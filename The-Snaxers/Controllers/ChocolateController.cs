using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TheSnaxers.Models;
using TheSnaxers.ViewModels;
using TheSnaxers.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;

namespace TheSnaxers.Controllers;

public class ChocolateController : Controller
{
    private readonly IBlobService _blobService;
    private readonly IProductService _productService;
    private readonly ICountryService _countryService;
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ChocolateController> _logger;

    public ChocolateController(
        IProductService productService, 
        ICountryService countryService, 
        IFavoriteService favoriteService,
        IBlobService blobService,
        UserManager<IdentityUser> userManager,
        ILogger<ChocolateController> logger)
    {
        _blobService = blobService;
        _productService = productService;
        _countryService = countryService;
        _favoriteService = favoriteService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? minCocoa)
    {
        // 1. Hämta produkterna från ProductService (som nu kör mot Cosmos DB)
        List<Product> products;
        if (!string.IsNullOrWhiteSpace(searchTerm) || minCocoa.HasValue)
        {
            _logger.LogInformation("Searching products with term '{SearchTerm}' and minCocoa {MinCocoa}", searchTerm, minCocoa);
            products = await _productService.SearchProductsAsync(searchTerm ?? "", minCocoa);
        }
        else
        {
            products = await _productService.GetAllProductsAsync();
        }

        _logger.LogInformation("Retrieved {ProductCount} products", products.Count);

        // 2. Hantera favoriter för den inloggade användaren
        var userId = _userManager.GetUserId(User);
        ViewBag.FavoriteIds = userId != null 
            ? (await _favoriteService.GetUserFavoritesAsync(userId)).Select(f => f.ProductId).ToList() 
            : new List<int>();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.MinCocoa = minCocoa;

        // 3. Mappa produkterna till ViewModels och berika med CountryInfo
    var viewModel = new List<ChocolateGalleryViewModel>();
    foreach (var p in products)
    {
        // TECH DEBT FIX: Vi använder p.Country direkt och skippar switch-satsen för kategorier
        var searchCountry = !string.IsNullOrWhiteSpace(p.Country) ? p.Country : "Sweden";

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
            Name = p.Name ?? "Okänt",
            Brand = p.Brand ?? "Okänt",
            CocoaPercentage = p.CocoaPercentage,
            Description = p.Description ?? "",
            Price = p.Price,
            // TECH DEBT FIX: Om ImageUrl är tom i DB används standardvärdet från ViewModel-klassen
            ImageUrl = !string.IsNullOrWhiteSpace(p.ImageUrl) ? p.ImageUrl : "/images/placeholder-choco.png",
            CountryName = countryInfo?.Name ?? p.Country ?? "Okänt",
            CountryCode = p.CountryCode,
            FlagUrl = countryInfo?.FlagUrl ?? ""
        });
    }

        return View(viewModel);
    }
    // GET: Chocolate/Create
public IActionResult Create()
{
    return View();
}

// POST: Chocolate/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Product product, IFormFile imageFile)
{
    // Ta bort ImageUrl från valideringen eftersom vi sätter den manuellt efter uppladdning
    ModelState.Remove("ImageUrl"); 
    ModelState.Remove("imageFile"); // Ibland klagar den på filen också om den inte finns i modellen

    if (ModelState.IsValid)
    {
        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                _logger.LogInformation("Uploading image {FileName} to Blob Storage", imageFile.FileName);
                
                using var stream = imageFile.OpenReadStream();
                // Här anropar vi din tjänst!
                var imageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName);
                
                // Spara URL:en från Azure i produktobjektet
                product.ImageUrl = imageUrl;
            }

            // Spara produkten i Cosmos DB via ProductService
            await _productService.AddProductAsync(product);
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError("", "Ett fel uppstod när chokladen skulle sparas.");
        }
    }

    return View(product);
}
}