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
        var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
        return View(favorites);
    }

    [HttpPost]
public async Task<IActionResult> Add(int productId, string returnUrl = "Chocolate") // Ändrat default till Chocolate
{
    var userId = _userManager.GetUserId(User);
    if (userId == null) return RedirectToAction("Index", "Chocolate");

    await _favoriteService.AddToFavoritesAsync(userId, productId);

    // Gå tillbaka till där användaren kom ifrån (nu med Chocolate som standard)
    if (returnUrl == "Favorite") return RedirectToAction("Index", "Favorite");
    
    return RedirectToAction("Index", "Chocolate");
}

[HttpPost]
public async Task<IActionResult> Remove(int productId, string returnUrl = "Chocolate") // Ändrat default till Chocolate
{
    var userId = _userManager.GetUserId(User);
    
    await _favoriteService.RemoveFromFavoritesAsync(userId, productId);

    // Om vi klickar "ta bort" inne i favoritlistan, stanna där. 
    // Annars, gå tillbaka till galleriet.
    if (returnUrl == "Favorite") return RedirectToAction("Index", "Favorite");

    return RedirectToAction("Index", "Chocolate");
}
}