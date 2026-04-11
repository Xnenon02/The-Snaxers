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
            // TODO: Ta bort category-switch när CosmosDB är på plats och Country alltid är ifyllt
            var searchCountry = !string.IsNullOrWhiteSpace(p.Country) ? p.Country : p.Category switch
            {
                "Mörk" => "France",
                "Vit" => "Switzerland",
                "Mjölk" => "Finland",
                "Ruby" => "Belgium",
            _   => "Sweden",
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
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CountryName = countryInfo?.Name ?? p.Country,
                FlagUrl = countryInfo?.FlagUrl ?? ""            });
        }

        return View(viewModel);
    }
}