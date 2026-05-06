using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace TheSnaxers.Controllers;

[Authorize]
public class WhoAmIController : Controller
{
    public IActionResult Index() => View();
}