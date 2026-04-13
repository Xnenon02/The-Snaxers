using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TheSnaxers.Services;
using TheSnaxers.Models;

namespace TheSnaxers.Controllers;

[Authorize] 
public class AdminChocolateController : Controller
{
    private readonly IProductService _productService;

    public AdminChocolateController(IProductService productService)
    {
        _productService = productService;
    }

    // 1. Listan på all choklad (Admin-vyn)
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllProductsAsync();
        return View(products);
    }

    // 2. Visa formuläret "Skapa ny"
    public IActionResult Create()
    {
        return View();
    }

    // 3. Ta emot datan från formuläret och spara
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        // 1. Fixa bilden FÖRST
        if (string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            product.ImageUrl = "/images/placeholder-choco.png"; 
        }

        // 2. Rensa eventuella valideringsfel för ImageUrl eftersom vi nu har satt ett värde
        ModelState.Remove("ImageUrl");

        if (ModelState.IsValid)
        {
            await _productService.AddProductAsync(product);
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    // 4. Visa formuläret (förifyllt)
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductByIdAsync(id); 
        if (product == null) return NotFound();
        
        return View(product);
    }

    // 5. Ta emot ändringarna
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product)
    {
        // 1. Fixa bilden FÖRST så att databasen aldrig får en tom sträng (NULL)
        if (string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            product.ImageUrl = "/images/placeholder-choco.png";
        }

        // 2. Berätta för valideringen att ImageUrl är okej nu
        ModelState.Remove("ImageUrl");

        if (ModelState.IsValid)
        {
            await _productService.UpdateProductAsync(product);
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    // 6. Radera produkt
    [HttpPost] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteProductAsync(id);
        return RedirectToAction(nameof(Index));
    }
}