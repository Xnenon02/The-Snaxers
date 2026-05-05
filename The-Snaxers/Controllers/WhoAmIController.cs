using Microsoft.AspNetCore.Mvc;

namespace TheSnaxers.Controllers;

public class WhoAmIController : Controller
{
    public IActionResult Index() => View();
}