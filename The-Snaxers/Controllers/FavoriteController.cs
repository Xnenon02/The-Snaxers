using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Services;

namespace TheSnaxers.Controllers;

// TODO: Verifiera att [Authorize] fungerar korrekt med Google OAuth (US3)
[Authorize]
public class FavoriteController : Controller
{
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<FavoriteController> _logger;

    public FavoriteController(
        IFavoriteService favoriteService, 
        UserManager<IdentityUser> userManager,
        ILogger<FavoriteController> logger)
    {
        _favoriteService = favoriteService;
        _userManager = userManager;
        _logger = logger;
    }

    [ResponseCache(NoStore = true, Duration = 0)]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Unauthorized access attempt to favorites index");
            return Challenge(); 
        }

        _logger.LogInformation("User {UserId} viewed their favorites", userId);
        var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
        return View(favorites);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, string returnUrl = "Chocolate")
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Unauthorized add favorite attempt for product {ProductId}", productId);
            return RedirectToAction("Index", "Chocolate");
        }

        _logger.LogInformation("User {UserId} added product {ProductId} to favorites", userId, productId);
        await _favoriteService.AddToFavoritesAsync(userId, productId);

        if (returnUrl == "Product") return RedirectToAction("Index", "Product");
        if (returnUrl == "Favorite") return RedirectToAction("Index", "Favorite");
        return RedirectToAction("Index", "Chocolate");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int productId, string returnUrl = "Chocolate")
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Unauthorized remove favorite attempt for product {ProductId}", productId);
            return Unauthorized();
        }

        _logger.LogInformation("User {UserId} removed product {ProductId} from favorites", userId, productId);
        await _favoriteService.RemoveFromFavoritesAsync(userId, productId);

        if (returnUrl == "Product") return RedirectToAction("Index", "Product");
        if (returnUrl == "Favorite") return RedirectToAction("Index", "Favorite");
        return RedirectToAction("Index", "Chocolate");
    }
}