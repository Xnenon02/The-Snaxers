using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // Behövs för UserManager
using TheSnaxers.Models;
using TheSnaxers.ViewModels;
using TheSnaxers.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Behövs för .Select()

namespace TheSnaxers.Controllers;

public class ChocolateController : Controller
{
    private readonly IProductService _productService;
    private readonly ICountryService _countryService;
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<IdentityUser> _userManager; // Lägg till denna

    // Uppdaterad Constructor med 4 tjänster
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

    public async Task<IActionResult> Index()
    {
        // 1. Hämta produkterna
        var products = await _productService.GetAllProductsAsync();

        // 2. Hantera favoriter (Hanitas logik)
        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            // Vi hämtar användarens favoriter och gör om dem till en lista med bara ID-nummer
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            ViewBag.FavoriteIds = favorites.Select(f => f.ProductId).ToList();
        }
        else
        {
            // Om ingen är inloggad skickar vi en tom lista så vyn inte kraschar
            ViewBag.FavoriteIds = new List<int>();
        }

        // 3. Mappa produkterna till din ViewModel
        var viewModel = new List<ChocolateGalleryViewModel>();

        foreach (var p in products)
        {
            // "Ful-hack" för länder
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

        // 4. Skicka din färdiga lista till vyn
        return View(viewModel);
    }
}