using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Services;
namespace TheSnaxers.Controllers;
[Authorize]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
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

    // [ValidateAntiForgeryToken] is intentionally removed from Add.
    // GET requests (post-login redirects from Identity) carry no antiforgery token,
    // so the attribute would cause HTTP 400 on every login redirect.
    // Instead, antiforgery is validated manually for POST requests only.
    // The unauthenticated favorite flow works as follows:
    //   1. User clicks favorite → redirected to login with returnUrl=/Favorite/Add?productId=...
    //   2. After login, Identity redirects back via GET → favorite is saved automatically
    [HttpGet] // Tillagt för att hantera redirect efter inloggning (förhindrar 404)
    [HttpPost]
    public async Task<IActionResult> Add(string productId, string returnUrl = "Chocolate", string? searchTerm = null, int? minCocoa = null)
    {
        // GET requests after login redirect — save favorite and redirect to gallery
        if (HttpMethods.IsGet(Request.Method))
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Chocolate");

            if (!string.IsNullOrEmpty(productId))
            {
                _logger.LogInformation("User {UserId} added product {ProductId} to favorites via GET (post-login redirect)", userId, productId);
                await _favoriteService.AddToFavoritesAsync(userId, productId);
            }

            // Skickar med sökparametrar för att bevara användarens filter
            return RedirectToAction("Index", "Chocolate", new { searchTerm, minCocoa });
        }

        // POST requests — validate antiforgery token manually
        try
        {
            var antiforgery = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
            await antiforgery.ValidateRequestAsync(HttpContext);
        }
        catch
        {
            _logger.LogWarning("Antiforgery validation failed for Add favorite, product {ProductId}", productId);
            return BadRequest();
        }

        var userIdPost = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdPost))
        {
            _logger.LogWarning("Unauthorized add favorite attempt for product {ProductId}", productId);
            return RedirectToAction("Index", "Chocolate");
        }

        _logger.LogInformation("User {UserId} added product {ProductId} to favorites", userIdPost, productId);
        await _favoriteService.AddToFavoritesAsync(userIdPost, productId);
        if (returnUrl == "Product") return RedirectToAction("Index", "Product");
        if (returnUrl == "Favorite") return RedirectToAction("Index", "Favorite");
        // Preserve search parameters when returning to Chocolate gallery
        // Only pass minCocoa if it has a value to avoid sending empty string
        return RedirectToAction("Index", "Chocolate", new { searchTerm, minCocoa = minCocoa.HasValue ? minCocoa : null });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string productId, string returnUrl = "Chocolate", string? searchTerm = null, int? minCocoa = null)
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
        // Preserve search parameters when returning to Chocolate gallery
        // Only pass minCocoa if it has a value to avoid sending empty string
        return RedirectToAction("Index", "Chocolate", new { searchTerm, minCocoa = minCocoa.HasValue ? minCocoa : null });
    }
}