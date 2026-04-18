using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TheSnaxers.Services;
using TheSnaxers.Models;

namespace TheSnaxers.Controllers;

[Authorize(Roles = "Admin")]
public class AdminChocolateController : Controller
{
    private readonly IProductService _productService;
    private readonly IBlobService _blobService;
    private readonly ILogger<AdminChocolateController> _logger;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly byte[][] ImageMagicBytes =
    [
        [0xFF, 0xD8, 0xFF],          // JPEG
        [0x89, 0x50, 0x4E, 0x47],    // PNG
        [0x52, 0x49, 0x46, 0x46],    // WEBP
    ];
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

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
        if (imageFile != null && imageFile.Length > 0)
        {
            var validationError = ValidateImageFile(imageFile);
            if (validationError != null)
            {
                ModelState.AddModelError("imageFile", validationError);
                return View(product);
            }

            using var stream = imageFile.OpenReadStream();
            product.ImageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName);
            _logger.LogInformation("Image uploaded for new product {ProductName}: {ImageUrl}", product.Name, product.ImageUrl);
        }

        ModelState.Remove("ImageUrl");

        if (ModelState.IsValid)
        {
            await _productService.AddProductAsync(product);
            _logger.LogInformation("Product created: {ProductName}", product.Name);
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
    public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
    {
        if (id != product.Id)
            return BadRequest("ID mismatch - Manipulation detekterad.");

        if (imageFile != null && imageFile.Length > 0)
        {
            var validationError = ValidateImageFile(imageFile);
            if (validationError != null)
            {
                ModelState.AddModelError("imageFile", validationError);
                return View(product);
            }

            // AC5 — Radera gamla bilden innan ny laddas upp
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                await _blobService.DeleteImageAsync(product.ImageUrl);
                _logger.LogInformation("Old image deleted for product {ProductId}: {ImageUrl}", id, product.ImageUrl);
            }

            using var stream = imageFile.OpenReadStream();
            product.ImageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName);
            _logger.LogInformation("New image uploaded for product {ProductId}: {ImageUrl}", id, product.ImageUrl);
        }

        ModelState.Remove("imageFile");

        if (ModelState.IsValid)
        {
            await _productService.UpdateProductAsync(product);
            _logger.LogInformation("Product updated: {ProductId}", id);
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product != null && !string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            // AC4 — Radera bilden från Blob Storage när produkten raderas
            await _blobService.DeleteImageAsync(product.ImageUrl);
            _logger.LogInformation("Image deleted from Blob Storage for product {ProductId}: {ImageUrl}", id, product.ImageUrl);
        }

        await _productService.DeleteProductAsync(id);
        _logger.LogInformation("Product deleted: {ProductId}", id);
        return RedirectToAction(nameof(Index));
    }

    // AC3 — Validering av filtyp och storlek
    private static string? ValidateImageFile(IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
            return "Filen är för stor. Max 2 MB tillåts.";

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            return "Otillåtet filformat. Endast .jpg, .png och .webp tillåts.";

        // Magic bytes-validering — kontrollera faktiskt filinnehåll
        using var stream = file.OpenReadStream();
        var header = new byte[4];
        stream.ReadExactly(header, 0, header.Length);
        
        var isValidImage = ImageMagicBytes.Any(magic =>
            header.Take(magic.Length).SequenceEqual(magic));

        if (!isValidImage)
            return "Filen verkar inte vara en giltig bild.";

        return null;
    }
}