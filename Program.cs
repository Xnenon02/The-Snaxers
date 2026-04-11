using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TheSnaxers.Data;
using TheSnaxers.Services;
using TheSnaxers.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=snaxers.db"));

builder.Services.AddScoped<TheSnaxers.Repositories.IFavoriteRepository, TheSnaxers.Repositories.FavoriteRepository>();
builder.Services.AddScoped<TheSnaxers.Services.IFavoriteService, TheSnaxers.Services.FavoriteService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICountryService, CountryService>();

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
        // TODO: Byt lokal ImageUrl mot Azure Blob Storage URL när US5 är klar
        // Lokalt: /images/products/filnamn.jpg
        // Produktion: https://snaxers.blob.core.windows.net/products/filnamn.jpg
        db.Products.AddRange(
            // Bas-choklader
            new TheSnaxers.Models.Product { Name = "Madagaskar Mörk", Description = "Fruktigt och komplex mörk choklad från Madagascar", Price = 109.90m, Category = "Mörk", CocoaPercentage = 75, Brand = "Valrhona", Country = "Madagascar", ImageUrl = "/images/products/madagascarDark.jpg" },
            new TheSnaxers.Models.Product { Name = "Peruansk Mjölkchoklad", Description = "Krämig mjölkchoklad med karamellnoter från Peru", Price = 79.90m, Category = "Mjölk", CocoaPercentage = 38, Brand = "Pacari", Country = "Peru", ImageUrl = "/images/products/peruvianMilk.jpg" },
            new TheSnaxers.Models.Product { Name = "Belgisk Vit", Description = "Klassisk krämig vit choklad från Belgien", Price = 69.90m, Category = "Vit", CocoaPercentage = 28, Brand = "Callebaut", Country = "Belgium", ImageUrl = "/images/products/belgianWhite.jpg" },

            // Spännande smaker
            new TheSnaxers.Models.Product { Name = "Pistagemousse", Description = "Lyxig mörk choklad fylld med pistagemousse från Sicilien", Price = 129.90m, Category = "Mörk", CocoaPercentage = 65, Brand = "Amedei", Country = "Italy", ImageUrl = "/images/products/italyPistage.jpeg" },
            new TheSnaxers.Models.Product { Name = "Yuzu & Vit Choklad", Description = "Japansk yuzu-citrus möter krämig vit choklad", Price = 119.90m, Category = "Vit", CocoaPercentage = 33, Brand = "Royce", Country = "Japan", ImageUrl = "/images/products/japanYuzu.jpg" },
            new TheSnaxers.Models.Product { Name = "Mörk Tryffel", Description = "Intensiv mörk choklad med tryffelkärna", Price = 89.90m, Category = "Mörk", CocoaPercentage = 72, Brand = "Valrhona", Country = "France", ImageUrl = "/images/products/darkTruffle.jpg" },
            new TheSnaxers.Models.Product { Name = "Hallon & Vit Choklad", Description = "Krämig vit choklad med hallonkräm", Price = 79.90m, Category = "Vit", CocoaPercentage = 30, Brand = "Lindt", Country = "Switzerland", ImageUrl = "/images/products/hallontryffel.jpg" },
            new TheSnaxers.Models.Product { Name = "Saltkaramell", Description = "Mjölkchoklad med flytande saltkaramell", Price = 69.90m, Category = "Mjölk", CocoaPercentage = 45, Brand = "Fazer", Country = "Finland", ImageUrl = "/images/products/saltkaramelltryffel.jpg" },
            new TheSnaxers.Models.Product { Name = "Ruby Choklad", Description = "Unik rosa choklad med bärsmak, utan tillsatta färger", Price = 99.90m, Category = "Ruby", CocoaPercentage = 47, Brand = "Callebaut", Country = "Belgium", ImageUrl = "/images/products/rubyChoklad.jpg" }
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