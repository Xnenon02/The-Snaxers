using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Azure.Identity;
using TheSnaxers.Data;
using TheSnaxers.Services;
using TheSnaxers.Repositories;
using Microsoft.Azure.Cosmos;
using TheSnaxers.Models;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
// KEY VAULT — AC2/AC3: Staging och Production hämtar hemligheter från Key Vault
// AC4: Kastar fel om KeyVault:Url saknas i staging/prod
// ===================================================
if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];

    if (string.IsNullOrEmpty(keyVaultUrl))
        throw new InvalidOperationException(
            $"[AC4] KeyVault:Url saknas för miljö '{builder.Environment.EnvironmentName}'. " +
            "Sätt miljövariabeln KeyVault__Url i Azure Container Apps.");

    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// ===================================================
// APPLICATION INSIGHTS — AC2/AC3: Staging och Production
// ===================================================
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString) && appInsightsConnectionString != "placeholder")
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();
builder.Services.AddLogging();

// ===================================================
// SQLITE — AC1: Development använder lokal SQLite och User Secrets
// Staging/Prod: Identity-databas hanteras separat (se tech debt)
// ===================================================
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=snaxers.db"));
}
else
{
    // TODO: Konfigurera Identity-databas för Staging/Prod i separat ticket
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=snaxers.db"));
}

// ===================================================
// COSMOS DB — AC1/AC2/AC3: Olika databaser per miljö
// Dev: TheSnaxersDb, Staging: TheSnaxersDb-staging, Prod: TheSnaxersDb
// AC4: Kastar fel om AccountEndpoint saknas i staging/prod
// ===================================================
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = configuration["CosmosDb:AccountEndpoint"];

    if (string.IsNullOrWhiteSpace(endpoint))
    {
        if (!builder.Environment.IsDevelopment())
            throw new InvalidOperationException(
                $"[AC4] CosmosDb:AccountEndpoint saknas för miljö '{builder.Environment.EnvironmentName}'.");

        throw new InvalidOperationException("CosmosDb:AccountEndpoint saknas i konfigurationen.");
    }

    // ⚠️ AccountKey ENDAST i Development — aldrig i staging/prod
    var accountKey = configuration["CosmosDb:AccountKey"];
    if (builder.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(accountKey))
        return new CosmosClient(endpoint, accountKey);

    // Staging/Prod — Managed Identity (Passwordless)
    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        TenantId = configuration["CosmosDb:TenantId"]
    });

    return new CosmosClient(endpoint, credential);
});

// Favoriter - Cosmos DB
builder.Services.AddScoped<TheSnaxers.Repositories.IFavoriteRepository, TheSnaxers.Repositories.CosmosFavoriteRepository>();
builder.Services.AddScoped<TheSnaxers.Services.IFavoriteService, TheSnaxers.Services.FavoriteService>();

// Produkter - Cosmos DB
builder.Services.AddScoped<IProductRepository, CosmosProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICountryService, CountryService>();

// Identity med rollstöd
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Säkerställ att SQLite-databasen finns för Identity
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapStaticAssets();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Skapa rollen Admin om den inte finns
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // 2. Kolla om din email finns, och gör den till Admin
    var adminEmail = "admin@snaxers.se"; 
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser != null && !(await userManager.IsInRoleAsync(adminUser, "Admin")))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();