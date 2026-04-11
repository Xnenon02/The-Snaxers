using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Azure.Identity;
using TheSnaxers.Data;
using TheSnaxers.Services;
using TheSnaxers.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
// KEY VAULT — Läser secrets från Azure Key Vault i produktion
// och från User Secrets lokalt i utveckling
// ===================================================
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential()); // Managed Identity i produktion
    }
}

// ===================================================
// APPLICATION INSIGHTS
// ===================================================
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Add services
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=snaxers.db"));

builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Seed testprodukter
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new TheSnaxers.Models.Product { Name = "Mörk Tryffel", Description = "Intensiv mörk choklad med tryffelkärna", Price = 89.90m, Category = "Mörk", CocoaPercentage = 72, Brand = "Valrhona", Country = "Frankrike", ImageUrl = "" },
            new TheSnaxers.Models.Product { Name = "Hallon & Vit Choklad", Description = "Krämig vit choklad med hallonkräm", Price = 79.90m, Category = "Vit", CocoaPercentage = 30, Brand = "Lindt", Country = "Schweiz", ImageUrl = "" },
            new TheSnaxers.Models.Product { Name = "Saltkaramell", Description = "Mjölkchoklad med flytande saltkaramell", Price = 69.90m, Category = "Mjölk", CocoaPercentage = 45, Brand = "Fazer", Country = "Finland", ImageUrl = "" }
        );
        db.SaveChanges();
    }
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