using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Azure.Identity;
using Azure.Storage.Blobs;
using TheSnaxers.Data;
using TheSnaxers.Services;
using TheSnaxers.Repositories;
using Microsoft.Azure.Cosmos;
using TheSnaxers.Models;
using Scalar.AspNetCore;
using TheSnaxers.Filters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;

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
var appInsightsConnStr = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnStr) && appInsightsConnStr != "placeholder")
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnStr;
    });
}

// Add services
builder.Services.AddControllersWithViews(options =>
{
    // 🔒 Tvingar validering av antiförfalskningstoken för alla POST-anrop globalt
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // <-- Denna gör oss immuna mot stora/små bokstäver!
});
builder.Services.AddLogging();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck<CosmosHealthCheck>("cosmos", tags: new[] { "ready" })
    .AddCheck<BlobHealthCheck>("blob", tags: new[] { "ready" });

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

    var accountKey = configuration["CosmosDb:AccountKey"];
    if (!string.IsNullOrWhiteSpace(accountKey))
        return new CosmosClient(endpoint, accountKey);

    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        TenantId = configuration["CosmosDb:TenantId"]
    });

    return new CosmosClient(endpoint, credential);
});

// BlobServiceClient
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = configuration["AzureStorage:BlobEndpoint"];
    if (!string.IsNullOrWhiteSpace(endpoint))
        return new BlobServiceClient(new Uri(endpoint), new DefaultAzureCredential());

    var connStr = configuration["AzureStorage:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(connStr))
        return new BlobServiceClient(connStr);

    return new BlobServiceClient(new Uri("https://localhost"), new DefaultAzureCredential());
});

// ===================================================
// REPOSITORIES
// ===================================================
var dbName = builder.Configuration["CosmosDb:DatabaseName"]
    ?? throw new InvalidOperationException("CosmosDb:DatabaseName saknas.");
var productsContainer = builder.Configuration["CosmosDb:ContainerName"]
    ?? throw new InvalidOperationException("CosmosDb:ContainerName saknas.");
var favoritesContainer = builder.Configuration["CosmosDb:FavoritesContainerName"] ?? "Favorites";
var cartsContainer = builder.Configuration["CosmosDb:CartContainerName"];

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

if (!string.IsNullOrEmpty(cartsContainer))
{
    builder.Services.AddScoped<ICartRepository>(sp =>
        new CosmosCartRepository(
            sp.GetRequiredService<CosmosClient>(),
            dbName,
            cartsContainer
        ));
}
else
{
    builder.Services.AddSingleton<ICartRepository, InMemoryCartRepositoryFallback>();
}

builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddCookiePolicy(options =>
{
    options.CheckConsentNeeded = context => false; // false = auth-cookies skickas alltid, oavsett cookie-samtycke
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ===================================================
// AUTENTISERING & BEHÖRIGHET (Google OAuth & JWT Bearer)
// ===================================================
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

// Hämta JWT-inställningar från Key Vault / User Secrets
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret saknas i konfigurationen!");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TheSnaxersAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TheSnaxersApp";

// Initiera autentiseringstjänsterna
var authBuilder = builder.Services.AddAuthentication();

// Lägg till Google om nycklarna finns
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.Scope.Add("profile");
        options.SaveTokens = true;
    });
}

// Lägg till JWT Bearer (Ligger utanför if-satsen så API-säkerheten alltid körs!)
authBuilder.AddJwtBearer("Bearer",options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        ValidateLifetime = true, // Kollar så stämpeln inte har gått ut

        ValidateIssuerSigningKey = true, // Validera signaturen med vår hemliga nyckel
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSecret))
    };
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.Use(async (context, next) =>
{
    var correlationId = Guid.NewGuid().ToString("N");
    context.Items["CorrelationId"] = correlationId;
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("CorrelationIdMiddleware");
    using (logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = correlationId }))
    {
        await next();
    }
});
app.UseRouting();

app.UseCookiePolicy(); 
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();



// ===================================================
// DOKUMENTATION — Scalar API-dokumentation (Development + Production)
// ===================================================
app.MapOpenApi();
app.MapScalarApiReference();

app.MapStaticAssets();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteJsonResponse });
// ===================================================
// INBYGGDA IDENTITY ENDPOINTS (För /login, /register osv)
// ===================================================
app.MapIdentityApi<IdentityUser>();
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

    if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
    try { await productService.GetAllProductsAsync(); } catch { }
}

app.Run();

static Task WriteJsonResponse(HttpContext ctx, HealthReport report)
{
    ctx.Response.ContentType = "application/json";
    var payload = System.Text.Json.JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            duration_ms = (int)e.Value.Duration.TotalMilliseconds
        })
    });
    return ctx.Response.WriteAsync(payload);
}

public class InMemoryCartRepositoryFallback : ICartRepository
{
    private readonly Dictionary<string, ShoppingCart> _carts = new();
    public async Task<ShoppingCart> GetCartByUserIdAsync(string userId) => _carts.TryGetValue(userId, out var cart) ? cart : new ShoppingCart { Id = userId, UserId = userId };
    public async Task SaveCartAsync(ShoppingCart cart) => _carts[cart.Id] = cart;
    public async Task ClearCartAsync(string userId) => _carts.Remove(userId);
}