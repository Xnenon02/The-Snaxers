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
// Only register when a real connection string is present — avoids crash in local Docker
// where no App Insights resource exists
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

// BlobServiceClient — used by BlobHealthCheck to verify storage connectivity
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = configuration["AzureStorage:BlobEndpoint"];
    if (!string.IsNullOrWhiteSpace(endpoint))
        return new BlobServiceClient(new Uri(endpoint), new DefaultAzureCredential());

    var connStr = configuration["AzureStorage:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(connStr))
        return new BlobServiceClient(connStr);

    // Dev fallback: no storage config present locally — BlobHealthCheck will report Unhealthy
    return new BlobServiceClient(new Uri("https://localhost"), new DefaultAzureCredential());
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

// CartRepository — använder CosmosDB om CosmosDb:CartContainerName är konfigurerat,
// annars InMemory (dev-fallback, cart nollställs vid omstart)
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

// Aktivera Session och säkerställ strikta cookie-inställningar
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;          // 🔒 Skyddar mot XSS
    options.Cookie.IsEssential = true;     // 🔒 Nödvändig för GDPR/funktion
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // 🔒 Bytte från Always till SameAsRequest för att stödja HTTP i Docker
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddCookiePolicy(options =>
{
    // Set to false — CheckConsentNeeded=true blocks Identity auth cookies and causes HTTP 400 on login
    options.CheckConsentNeeded = context => true; 
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

// Säkra även standard-identitetscookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // 60 minuter istället för 20 som ni diskuterade
    options.SlidingExpiration = true;
    
    // Säkerställ att sökvägarna pekar mot standard Identity-sidorna
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
});

// Identity - SQLite tills VM är uppsatt
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ===================================================
// GOOGLE OAUTH — User Secrets lokalt, Key Vault i produktion
// ===================================================
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;

            // Hämta profilbild och namn från Google
            options.Scope.Add("profile");
            options.SaveTokens = true;
        });
}


var app = builder.Build();

// Säkerställ att SQLite-databasen finns för Identity
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// Configure pipeline
// Måste ligga FÖRE UseHttpsRedirection så att X-Forwarded-Proto:https
// från Container Apps-proxyn känns igen och redirect-loopen undviks
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
app.UseRouting();
app.UseCookiePolicy(); // Applies cookie consent and security policy
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Generates a per-request correlation ID and pushes it onto the logger scope
app.Use(async (context, next) =>
{
    var correlationId = Guid.NewGuid().ToString("N");
    context.Items["CorrelationId"] = correlationId;

    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("CorrelationIdMiddleware");

    using (logger.BeginScope(new Dictionary<string, object>
    {
        ["RequestId"] = correlationId
    }))
    {
        await next();
    }
});

// OpenAPI/Swagger — endast i Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapStaticAssets();

// Liveness — answers "is this process alive", no dependency checks
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness — answers "is this replica ready to serve traffic"
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("ready")
});

// Diagnostic — full JSON breakdown for humans and dashboards
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteJsonResponse
});

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

// Warm up product cache on startup to avoid slow first page load
using (var scope = app.Services.CreateScope())
{
    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
    try
    {
        await productService.GetAllProductsAsync();
        app.Logger.LogInformation("Product cache warmed up on startup.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Cache warm-up failed — will load on first request.");
    }
}

app.Run();

// Writes a JSON health report for the diagnostic /health endpoint
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
    public async Task<ShoppingCart> GetCartByUserIdAsync(string userId) => 
        _carts.TryGetValue(userId, out var cart) ? cart : new ShoppingCart { Id = userId, UserId = userId };
    public async Task SaveCartAsync(ShoppingCart cart) => _carts[cart.Id] = cart;
    public async Task ClearCartAsync(string userId) => _carts.Remove(userId);
}