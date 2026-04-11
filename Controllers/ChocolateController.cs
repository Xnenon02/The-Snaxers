using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TheSnaxers.Models;
using TheSnaxers.ViewModels;
using TheSnaxers.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace TheSnaxers.Controllers;

public class ChocolateController : Controller
{
    private readonly IProductService _productService;
    private readonly ICountryService _countryService;
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<IdentityUser> _userManager;

    public ChocolateController(
        IProductService productService, 
        ICountryService countryService, 
        IFavoriteService favoriteService,
        UserManager<IdentityUser> userManager)
    {
        _productService = productService;
        _countryService = countryService;
        _favoriteService = favoriteService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? minCocoa)
    {
        // 1. Hämta produkterna med sökning/filtrering
        List<Product> products;

        if (!string.IsNullOrWhiteSpace(searchTerm) || minCocoa.HasValue)
            products = await _productService.SearchProductsAsync(searchTerm!, minCocoa);
        else
            products = await _productService.GetAllProductsAsync();

        // 2. Hantera favoriter
        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            ViewBag.FavoriteIds = favorites.Select(f => f.ProductId).ToList();
        }
        else
        {
            ViewBag.FavoriteIds = new List<int>();
        }

        ViewBag.SearchTerm = searchTerm;
        ViewBag.MinCocoa = minCocoa;

        // 3. Mappa till ViewModel
        var viewModel = new List<ChocolateGalleryViewModel>();

        foreach (var p in products)
        {
            var searchCountry = p.Category switch
            {
                "Mörk" => "Ecuador",
                "Vit" => "Belgium",
                "Mjölk" => "Switzerland",
                _ => p.Category
            };

            var countryInfo = await _countryService.GetCountryInfoAsync(searchCountry);

            viewModel.Add(new ChocolateGalleryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                CountryName = countryInfo?.Name ?? "Okänt land",
                FlagUrl = countryInfo?.FlagUrl ?? "/images/world-icon.png"
            });
        }

        return View(viewModel);
    }
}