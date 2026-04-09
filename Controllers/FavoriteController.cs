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
    public async Task<IActionResult> Add(int productId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Index", "Product");

        await _favoriteService.AddToFavoritesAsync(userId, productId);

        return RedirectToAction("Index", "Product");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int productId, string returnUrl = "Favorite")
    {
        var userId = _userManager.GetUserId(User);
        
        await _favoriteService.RemoveFromFavoritesAsync(userId, productId);

        if (returnUrl == "Product") return RedirectToAction("Index", "Product");

        return RedirectToAction("Index");
    }
}