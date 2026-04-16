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
    private readonly ILogger<AdminChocolateController> _logger;

    public AdminChocolateController(IProductService productService, IBlobService blobService, ILogger<AdminChocolateController> logger)
    {
        _productService = productService;
        _blobService = blobService;
        _logger = logger;
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
        _logger.LogInformation("Admin attempt to create product: {ProductName}", product.Name);

        if (imageFile != null && imageFile.Length > 0)
        {
            _logger.LogInformation("Image detected for {ProductName}. Starting upload to Blob Storage.", product.Name);
            
            // Öppna en ström till filen och skicka till molnet!
            using var stream = imageFile.OpenReadStream();
            string imageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName);
            
            // Spara den nya moln-URL:en i produkten
            product.ImageUrl = imageUrl;
            
            _logger.LogInformation("Image upload successful for {ProductName}. URL: {ImageUrl}", product.Name, imageUrl);
        }
        
        ModelState.Remove("ImageUrl"); // Se till att valideringen inte klagar på URL:en

        if (ModelState.IsValid)
        {
            await _productService.AddProductAsync(product);
            _logger.LogInformation("Successfully added {ProductName} to the database.", product.Name);
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Validation failed for creating product: {ProductName}", product.Name);
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
    public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile) // Lade till imageFile här
    {
        _logger.LogInformation("Admin attempt to edit product ID: {ProductId}", id);

        if (id != product.Id)
        {
            _logger.LogError("ID mismatch during edit: URL ID {UrlId} != Model ID {ModelId}", id, product.Id);
            return BadRequest("ID mismatch - Manipulation detekterad.");
        }

        if (imageFile != null && imageFile.Length > 0)
        {
            _logger.LogInformation("Updating image for product ID: {ProductId}", id);
            
            // Ladda upp den nya bilden
            using var stream = imageFile.OpenReadStream();
            string imageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName);
            
            // Uppdatera URL:en till den nya
            product.ImageUrl = imageUrl;

            _logger.LogInformation("New image URL for product {ProductId}: {ImageUrl}", id, imageUrl);
        }
        // Om imageFile är null behålls den existerande product.ImageUrl (från den dolda inputen i vyn)

        ModelState.Remove("imageFile"); // Validera inte själva filen som en del av modellen

        if (ModelState.IsValid)
        {
            await _productService.UpdateProductAsync(product);
            _logger.LogInformation("Successfully updated product ID: {ProductId}", id);
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Validation failed for editing product ID: {ProductId}", id);
        return View(product);
    }

    [HttpPost] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogWarning("Admin is deleting product ID: {ProductId}", id);
        await _productService.DeleteProductAsync(id);
        _logger.LogInformation("Product ID: {ProductId} deleted successfully.", id);
        return RedirectToAction(nameof(Index));
    }
}