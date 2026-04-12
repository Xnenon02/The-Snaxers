using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Models;

namespace TheSnaxers.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Home page visited");
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
    // TODO: Ta bort eller skydda med [Authorize(Roles="Admin")] i produktion
    // ===================================================
    [Route("api/test-error")]
    public IActionResult TestError()
    {
        _logger.LogWarning("Test error endpoint triggered");
        throw new Exception("Detta är ett test-fel för Application Insights demonstration 🍫");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}