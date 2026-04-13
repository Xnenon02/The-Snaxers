using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TheSnaxers.Services;
using TheSnaxers.Models;

namespace TheSnaxers.Controllers;

[Authorize] // Bara inloggade får vara här!
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
    // Om ImageUrl är tom eller bara innehåller mellanslag
    if (string.IsNullOrWhiteSpace(product.ImageUrl))
    {
        // Här sätter vi din placeholder-sökväg
        product.ImageUrl = "/images/placeholder-choco.png"; 
    }

    if (ModelState.IsValid)
    {
        await _productService.AddProductAsync(product);
        return RedirectToAction(nameof(Index));
    }
    return View(product);
}
// Visa formuläret (förifyllt)
public async Task<IActionResult> Edit(int id)
{
    var product = await _productService.GetProductByIdAsync(id); 
    if (product == null) return NotFound();
    
    return View(product);
}

// Ta emot ändringarna
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(Product product)
{
    if (ModelState.IsValid)
    {
        await _productService.UpdateProductAsync(product);
        return RedirectToAction(nameof(Index));
    }
    return View(product);
}
public async Task<IActionResult> Delete(int id)
{
    await _productService.DeleteProductAsync(id);
    return RedirectToAction(nameof(Index));
}
}