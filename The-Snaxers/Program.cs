using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Azure.Identity;
using TheSnaxers.Data;
using TheSnaxers.Services;
using TheSnaxers.Repositories;
using Microsoft.Azure.Cosmos;
using TheSnaxers.Models;
using Scalar.AspNetCore;
using TheSnaxers.Filters;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
// KEY VAULT — Azure Key Vault i produktion, User Secrets lokalt
// ===================================================
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }
}

// ===================================================
// APPLICATION INSIGHTS
// ===================================================
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString) && appInsightsConnectionString != "placeholder")
{
    // TODO: Lägg till riktig ConnectionString i Azure Key Vault när Tom satt upp miljön
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();
builder.Services.AddLogging();

// Registrera ApiKeyFilter som Singleton — bättre prestanda då det är stateless
builder.Services.AddSingleton<ApiKeyFilter>();

// ===================================================
// SQLITE — Identity-databas
// ===================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=snaxers.db"));

// ===================================================
// SWAGGER / OPENAPI — .NET 10 inbyggd OpenAPI
// ===================================================
builder.Services.AddOpenApi();

// Cosmos DB client
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = configuration["CosmosDb:AccountEndpoint"];

    if (string.IsNullOrWhiteSpace(endpoint))
        throw new InvalidOperationException("CosmosDb:AccountEndpoint saknas i konfigurationen.");

    // Dual-mode: use account key if available (local Docker), otherwise use Managed Identity (Azure production)
    var accountKey = configuration["CosmosDb:AccountKey"];
    if (!string.IsNullOrWhiteSpace(accountKey))
        return new CosmosClient(endpoint, accountKey);

    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        TenantId = configuration["CosmosDb:TenantId"]
    });

    return new CosmosClient(endpoint, credential);
});

// ===================================================
// REPOSITORIES — DI Cleanup (punkt 5)
// Containernamn skickas in direkt, IConfiguration behövs inte i repositories
// ===================================================
var dbName = builder.Configuration["CosmosDb:DatabaseName"]
    ?? throw new InvalidOperationException("CosmosDb:DatabaseName saknas.");
var productsContainer = builder.Configuration["CosmosDb:ContainerName"]
    ?? throw new InvalidOperationException("CosmosDb:ContainerName saknas.");
var favoritesContainer = builder.Configuration["CosmosDb:FavoritesContainerName"] ?? "Favorites";

builder.Services.AddScoped<IProductRepository>(sp =>
    new CosmosProductRepository(
        sp.GetRequiredService<CosmosClient>(),
        dbName,
        productsContainer,
        sp.GetRequiredService<ILogger<CosmosProductRepository>>()
    ));

builder.Services.AddScoped<IFavoriteRepository>(sp =>
    new CosmosFavoriteRepository(
        sp.GetRequiredService<CosmosClient>(),
        dbName,
        favoritesContainer,
        productsContainer,
        sp.GetRequiredService<ILogger<CosmosFavoriteRepository>>()
    ));

builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICountryService, CountryService>();

// Identity - SQLite tills VM är uppsatt
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
app.UseAuthentication();
app.UseAuthorization();

// OpenAPI/Swagger — endast i Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapStaticAssets();
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    var adminEmail = builder.Configuration["AdminSettings:Email"];
    var adminPassword = builder.Configuration["AdminSettings:Password"];

    // 3. Null-check: Kör bara om vi faktiskt har uppgifterna
    if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword)) 
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new IdentityUser 
            { 
                UserName = adminEmail, 
                Email = adminEmail, 
                EmailConfirmed = true 
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"System: Admin-användare {adminEmail} skapad.");
            }
        }
    }
    else 
    {
        Console.WriteLine("System: Admin-uppgifter saknas i konfigurationen (User Secrets). Hoppar över seeding.");
    }
}

app.Run();