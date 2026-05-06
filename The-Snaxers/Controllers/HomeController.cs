using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.ApplicationInsights;

namespace TheSnaxers.Controllers;

[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)] // Förhindrar cachning av fel och sidvisningar
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TelemetryClient? _telemetry;

    public HomeController(ILogger<HomeController> logger, TelemetryClient? telemetry = null)
    {
        _logger = logger;
        _telemetry = telemetry;
    }

    public IActionResult Index()
    {
        _logger.LogDebug("HomeController.Index invoked");
        _logger.LogInformation("Home page visited");

        // Track home page views as a pre-aggregated custom metric — cheap at any volume
        _telemetry?.GetMetric("home-page-views").TrackValue(1);

        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogInformation("Privacy page visited");
        return View();
    }

    // ===================================================
    // TEST-ENDPOINT FÖR APPLICATION INSIGHTS FELSÖKNING
    // Används för att demonstrera spårning i App Insights
    // ===================================================
    [Authorize(Roles = "Admin")]
    [Route("api/test-error")]
    public IActionResult TestError()
    {
        _logger.LogWarning("Test error endpoint triggered");
        throw new Exception("Detta är ett test-fel för Application Insights demonstration 🫠");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}