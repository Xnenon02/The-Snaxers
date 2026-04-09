using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Models;
using TheSnaxers.ViewModels;
using TheSnaxers.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheSnaxers.Controllers;

public class ChocolateController : Controller
{
    // Dessa fält behövs för att controllern ska kunna använda tjänsterna
    private readonly IProductService _productService;
    private readonly ICountryService _countryService;

    // Här "injiceras" tjänsterna (Constructor)
    public ChocolateController(IProductService productService, ICountryService countryService)
    {
        _productService = productService;
        _countryService = countryService;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Hämta produkterna
        var products = await _productService.GetAllProductsAsync();

        // 2. Mappa produkterna till din ViewModel
        var viewModel = new List<ChocolateGalleryViewModel>();

        foreach (var p in products)
        {
            // Vi hämtar flaggan. 
            // OBS: Se till att p.Category faktiskt innehåller ett land (t.ex. "Belgium")
            var countryInfo = await _countryService.GetCountryInfoAsync(p.Category);

            viewModel.Add(new ChocolateGalleryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                CountryName = countryInfo?.Name ?? "Okänt land",
                FlagUrl = countryInfo?.FlagUrl ?? "/images/world-icon.png" // AC3 fail-safe
            });
        }

        // 3. Skicka din färdiga lista till vyn
        return View(viewModel);
    }
}