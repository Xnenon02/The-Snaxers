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
        if (userId == null) return RedirectToAction("Index", "Home");

        var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
        return View(favorites);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, string returnUrl = "Chocolate")
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Index", "Chocolate");

        await _favoriteService.AddToFavoritesAsync(userId, productId);

        if (returnUrl == "Product") return RedirectToAction("Index", "Product");
        if (returnUrl == "Favorite") return RedirectToAction("Index", "Favorite");
        return RedirectToAction("Index", "Chocolate");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int productId, string returnUrl = "Chocolate")
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Index", "Home");

        await _favoriteService.RemoveFromFavoritesAsync(userId, productId);

        if (returnUrl == "Product") return RedirectToAction("Index", "Product");
        if (returnUrl == "Favorite") return RedirectToAction("Index", "Favorite");
        return RedirectToAction("Index", "Chocolate");
    }
}