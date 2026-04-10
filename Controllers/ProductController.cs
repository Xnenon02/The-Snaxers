using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TheSnaxers.Models;
using TheSnaxers.Services;

namespace TheSnaxers.Controllers;

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

    public async Task<IActionResult> Index(string? searchTerm, int? minCocoa)
    {
        List<Product> products;

        if (!string.IsNullOrWhiteSpace(searchTerm) || minCocoa.HasValue)
            products = await _productService.SearchProductsAsync(searchTerm!, minCocoa);
        else
            products = await _productService.GetAllProductsAsync();

        var userId = _userManager.GetUserId(User);
        var favoriteIds = userId != null
            ? (await _favoriteService.GetUserFavoritesAsync(userId))
                .Select(f => f.ProductId)
                .ToList()
            : new List<int>();

        ViewBag.FavoriteIds = favoriteIds;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.MinCocoa = minCocoa;

        return View(products);
    }
}