using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TheSnaxers.Data;
using TheSnaxers.Services;
using TheSnaxers.Repositories;
using TheSnaxers.Filters;

namespace TheSnaxers.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSnaxersInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Application Insights
            var appInsightsConnStr = configuration["ApplicationInsights:ConnectionString"];
            if (!string.IsNullOrEmpty(appInsightsConnStr) && appInsightsConnStr != "placeholder")
            {
                services.AddApplicationInsightsTelemetry(options => { options.ConnectionString = appInsightsConnStr; });
            }

            // Health Checks
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddCheck<CosmosHealthCheck>("cosmos", tags: new[] { "ready" })
                .AddCheck<BlobHealthCheck>("blob", tags: new[] { "ready" });

            // Identity DB & Stores
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=snaxers.db"));
            
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<IIdentitySeeder, IdentitySeeder>();
            services.AddSingleton<ApiKeyFilter>();

            return services;
        }

        public static IServiceCollection AddSnaxersStorageAndRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            // Cosmos DB Client
            services.AddSingleton(sp =>
            {
                var endpoint = configuration["CosmosDb:AccountEndpoint"] ?? throw new InvalidOperationException("CosmosDb:AccountEndpoint saknas.");
                var accountKey = configuration["CosmosDb:AccountKey"];
                
                if (!string.IsNullOrWhiteSpace(accountKey))
                    return new CosmosClient(endpoint, accountKey);

                return new CosmosClient(endpoint, new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    TenantId = configuration["CosmosDb:TenantId"]
                }));
            });

            // Blob Storage Client
            services.AddSingleton(sp =>
            {
                var endpoint = configuration["AzureStorage:BlobEndpoint"];
                if (!string.IsNullOrWhiteSpace(endpoint))
                    return new BlobServiceClient(new Uri(endpoint), new DefaultAzureCredential());

                var connStr = configuration["AzureStorage:ConnectionString"];
                if (!string.IsNullOrWhiteSpace(connStr))
                    return new BlobServiceClient(connStr);

                return new BlobServiceClient(new Uri("https://localhost"), new DefaultAzureCredential());
            });

            // Repositories setup
            var dbName = configuration["CosmosDb:DatabaseName"] ?? throw new InvalidOperationException("CosmosDb:DatabaseName saknas.");
            var productsContainer = configuration["CosmosDb:ContainerName"] ?? throw new InvalidOperationException("CosmosDb:ContainerName saknas.");
            var favoritesContainer = configuration["CosmosDb:FavoritesContainerName"] ?? "Favorites";
            var cartsContainer = configuration["CosmosDb:CartContainerName"];

            services.AddScoped<IProductRepository>(sp => new CosmosProductRepository(sp.GetRequiredService<CosmosClient>(), dbName, productsContainer, sp.GetRequiredService<ILogger<CosmosProductRepository>>()));
            services.AddScoped<IFavoriteRepository>(sp => new CosmosFavoriteRepository(sp.GetRequiredService<CosmosClient>(), dbName, favoritesContainer, productsContainer, sp.GetRequiredService<ILogger<CosmosFavoriteRepository>>()));

            if (!string.IsNullOrEmpty(cartsContainer))
                services.AddScoped<ICartRepository>(sp => new CosmosCartRepository(sp.GetRequiredService<CosmosClient>(), dbName, cartsContainer));
            else
                services.AddSingleton<ICartRepository, InMemoryCartRepositoryFallback>();

            // Application Services
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IBlobService, BlobService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<ICountryService, CountryService>();

            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            return services;
        }

        public static IServiceCollection AddSnaxersSecurityAndAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var googleClientId = configuration["Authentication:Google:ClientId"];
            var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
            var jwtSecret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret saknas!");
            var jwtIssuer = configuration["Jwt:Issuer"] ?? "TheSnaxersAPI";
            var jwtAudience = configuration["Jwt:Audience"] ?? "TheSnaxersApp";

            var authBuilder = services.AddAuthentication();

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

            authBuilder.AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

            // Cookies & Sessions
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            services.AddCookiePolicy(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            services.ConfigureApplicationCookie(options =>
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

            return services;
        }
    }
}