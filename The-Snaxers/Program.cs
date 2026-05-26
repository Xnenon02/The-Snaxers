using Azure.Identity;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TheSnaxers.Data;
using TheSnaxers.Services;
using TheSnaxers.Extensions;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Key Vault i produktion
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
    }
}

// Global MVC Setup
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddLogging();
builder.Services.AddOpenApi();

// ===================================================
// VÅRA NYA SNYGGA EXTENSION-METODER 🚀
// ===================================================
builder.Services.AddSnaxersInfrastructure(builder.Configuration);
builder.Services.AddSnaxersStorageAndRepositories(builder.Configuration);
builder.Services.AddSnaxersSecurityAndAuth(builder.Configuration);

var app = builder.Build();

// Initiera Identity Databasen lokalt
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

// Correlation ID / Trace Middleware
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

// API Dokumentation
app.MapOpenApi();
app.MapScalarApiReference();
app.MapStaticAssets();

// Health Checks Slutpunkter
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteJsonResponse });

app.MapIdentityApi<IdentityUser>();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.MapRazorPages();

// ===================================================
// SEEDNING AV SÄKERHET OCH CACHE-WARMUP 🧼
// ===================================================
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IIdentitySeeder>();
    await seeder.SeedAdminAndRolesAsync();
}

using (var scope = app.Services.CreateScope())
{
    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
    try { await productService.GetAllProductsAsync(); } catch { }
}

app.Run();

// Hjälpmetod för JSON health-svar
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