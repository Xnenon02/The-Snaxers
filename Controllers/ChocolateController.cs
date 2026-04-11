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
        List<Product> products;

        if (!string.IsNullOrWhiteSpace(searchTerm) || minCocoa.HasValue)
            products = await _productService.SearchProductsAsync(searchTerm!, minCocoa);
        else
            products = await _productService.GetAllProductsAsync();

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

        var viewModel = new List<ChocolateGalleryViewModel>();

        foreach (var p in products)
        {
            // Använd Country-fältet direkt — category-hacket tas bort när CosmosDB är på plats
            var searchCountry = !string.IsNullOrWhiteSpace(p.Country) ? p.Country : p.Category switch
            {
                "Mörk" => "Ecuador",
                "Vit" => "Belgium",
                "Mjölk" => "Switzerland",
                "Ruby" => "Belgium",
                _ => "Sweden" // Default om inget land hittas
            };

            CountryInfo? countryInfo = null;
            try
            {
                countryInfo = await _countryService.GetCountryInfoAsync(searchCountry);
            }
            catch
            {
                // Om API:et failar, använd default
            }

            viewModel.Add(new ChocolateGalleryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Brand = p.Brand,
                CocoaPercentage = p.CocoaPercentage,
                ImageUrl = p.ImageUrl,
                CountryName = countryInfo?.Name ?? p.Country,
                FlagUrl = countryInfo?.FlagUrl ?? "/images/flag-placeholder.svg"            });
        }

        return View(viewModel);
    }
}