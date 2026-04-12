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
    private readonly ILogger<ChocolateController> _logger;

    public ChocolateController(
        IProductService productService, 
        ICountryService countryService, 
        IFavoriteService favoriteService,
        UserManager<IdentityUser> userManager,
        ILogger<ChocolateController> logger)
    {
        _productService = productService;
        _countryService = countryService;
        _favoriteService = favoriteService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? minCocoa)
    {
        // 1. Hämta produkterna
        List<Product> products;
        if (!string.IsNullOrWhiteSpace(searchTerm) || minCocoa.HasValue)
        {
            _logger.LogInformation("Searching products with term '{SearchTerm}' and minCocoa {MinCocoa}", searchTerm, minCocoa);
            products = await _productService.SearchProductsAsync(searchTerm!, minCocoa);
        }
        else
        {
            products = await _productService.GetAllProductsAsync();
        }

        _logger.LogInformation("Retrieved {ProductCount} products", products.Count);

        // 2. Hantera favoriter
        var userId = _userManager.GetUserId(User);
        ViewBag.FavoriteIds = userId != null 
            ? (await _favoriteService.GetUserFavoritesAsync(userId)).Select(f => f.ProductId).ToList() 
            : new List<int>();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.MinCocoa = minCocoa;

        // 3. Mappa till ViewModel
        var viewModel = new List<ChocolateGalleryViewModel>();
        foreach (var p in products)
        {
            // TODO: Ta bort category-switch när CosmosDB är på plats och Country alltid är ifyllt
            var searchCountry = !string.IsNullOrWhiteSpace(p.Country) ? p.Country : p.Category switch
            {
                "Mörk" => "France",
                "Vit" => "Switzerland",
                "Mjölk" => "Finland",
                "Ruby" => "Belgium",
                _ => "Sweden"
            };

            CountryInfo? countryInfo = null;
            try 
            { 
                countryInfo = await _countryService.GetCountryInfoAsync(searchCountry); 
            }
            catch (Exception ex)
            { 
                _logger.LogWarning(ex, "Failed to fetch country info for {Country}", searchCountry);
            }

            viewModel.Add(new ChocolateGalleryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Brand = p.Brand,
                CocoaPercentage = p.CocoaPercentage,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CountryName = countryInfo?.Name ?? p.Country ?? "Okänt",
                FlagUrl = countryInfo?.FlagUrl ?? ""
            });
        }
        return View(viewModel);
    }
}