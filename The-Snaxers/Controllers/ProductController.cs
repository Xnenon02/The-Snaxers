using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TheSnaxers.Models;
using TheSnaxers.Services;

namespace TheSnaxers.Controllers;
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)] // Förhindrar cachning av produktlistor och detaljer

public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<IdentityUser> _userManager;

    public ProductController(
        IProductService productService,
        IFavoriteService favoriteService,
        UserManager<IdentityUser> userManager)
    {
        _productService = productService;
        _favoriteService = favoriteService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllProductsAsync();

        var userId = _userManager.GetUserId(User);
        var favoriteIds = userId != null
            ? (await _favoriteService.GetUserFavoritesAsync(userId))
                .Select(f => f.ProductId)
                .ToList()
            : new List<string>();

        ViewBag.FavoriteIds = favoriteIds;

        return View(products);
    }
}