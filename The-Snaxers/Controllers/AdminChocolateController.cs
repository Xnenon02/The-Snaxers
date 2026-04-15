using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TheSnaxers.Services;
using TheSnaxers.Models;

namespace TheSnaxers.Controllers;

[Authorize] 
public class AdminChocolateController : Controller
{
    private readonly IProductService _productService;
    private readonly IBlobService _blobService;

    public AdminChocolateController(IProductService productService, IBlobService blobService)
    {
        _productService = productService;
        _blobService = blobService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllProductsAsync();
        return View(products);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
{
    if (imageFile != null && imageFile.Length > 0)
    {
        // Öppna en ström till filen och skicka till molnet!
        using var stream = imageFile.OpenReadStream();
        string imageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName);
        
        // Spara den nya moln-URL:en i produkten
        product.ImageUrl = imageUrl;
    }
    
    ModelState.Remove("ImageUrl"); // Se till att valideringen inte klagar på URL:en

    if (ModelState.IsValid)
    {
        await _productService.AddProductAsync(product);
        return RedirectToAction(nameof(Index));
    }
    return View(product);
}

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductByIdAsync(id); 
        if (product == null) return NotFound();
        
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product) // Lade till int id här
    {
        // HANITAS SÄKERHETSKONTROLL: Matchar id i URL:en med id i bodyn?
        if (id != product.Id)
        {
            return BadRequest("ID mismatch - Manipulation detekterad.");
        }

        if (ModelState.IsValid)
        {
            await _productService.UpdateProductAsync(product);
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    [HttpPost] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteProductAsync(id);
        return RedirectToAction(nameof(Index));
    }
}