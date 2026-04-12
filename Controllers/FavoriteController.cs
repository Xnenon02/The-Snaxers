using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Services;

namespace TheSnaxers.Controllers;

[Authorize]
public class FavoriteController : Controller
{
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<IdentityUser> _userManager;

    public FavoriteController(IFavoriteService favoriteService, UserManager<IdentityUser> userManager)
    {
        _favoriteService = favoriteService;
        _userManager = userManager;
    }

    [ResponseCache(NoStore = true, Duration = 0)]
   public async Task<IActionResult> Index()
{
    var userId = _userManager.GetUserId(User);
    
    // Fix: Om userId är null, skicka användaren till Login eller hantera felet
    if (string.IsNullOrEmpty(userId))
    {
        return Challenge(); // Eller RedirectToAction("Login", "Account");
    }

    var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
    return View(favorites);
}

    [HttpPost]
public async Task<IActionResult> Add(int productId, string returnUrl = "Favorite")
{
    var userId = _userManager.GetUserId(User);
    if (userId == null) return RedirectToAction("Index", "Chocolate"); // Ändrat från Product

    await _favoriteService.AddToFavoritesAsync(userId, productId);

    // Om vi skickade med "Chocolate" som returnUrl, gå dit!
    if (returnUrl == "Chocolate") return RedirectToAction("Index", "Chocolate");

    return RedirectToAction("Index", "Product"); // Fallback
}

    [HttpPost]
public async Task<IActionResult> Remove(int productId, string returnUrl = "Chocolate")
{
    var userId = _userManager.GetUserId(User);

    // Fix: Kontrollera att userId faktiskt har ett värde
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized();
    }

    await _favoriteService.RemoveFromFavoritesAsync(userId, productId);
    
    if (returnUrl == "Favorite") return RedirectToAction("Index", "Favorite");
    return RedirectToAction("Index", "Chocolate");
}
}