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
// KEY VAULT — Azure Key Vault i produktion, User Secrets lokalt
// ===================================================
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        // Managed Identity — ingen hårdkodad connection string
        // TODO: Sätt KeyVault:Url i Azure Container Apps miljövariabler
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

// SQLite - används endast för Identity tills VM är uppsatt
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=snaxers.db"));

// Cosmos DB client
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = configuration["CosmosDb:AccountEndpoint"];

    if (string.IsNullOrWhiteSpace(endpoint))
        throw new InvalidOperationException("CosmosDb:AccountEndpoint saknas i konfigurationen.");

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

// Produkter - gammal lokal SQLite-version sparad men kommenterad
// builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICountryService, CountryService>();

// Identity - SQLite tills VM är uppsatt
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();