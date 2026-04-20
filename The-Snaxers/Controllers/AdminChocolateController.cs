using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TheSnaxers.Services;
using TheSnaxers.Models;
using Microsoft.AspNetCore.Identity;

namespace TheSnaxers.Controllers;

[Authorize(Roles = "Admin")]
public class AdminChocolateController : Controller
{
    private readonly IProductService _productService;
    private readonly IBlobService _blobService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AdminChocolateController> _logger;

    public AdminChocolateController(
        IProductService productService, 
        IBlobService blobService, 
        UserManager<IdentityUser> userManager,
        ILogger<AdminChocolateController> logger)
    {
        _productService = productService;
        _blobService = blobService;
        _userManager = userManager;
        _logger = logger;
    }
    
    // AC3 — Tillåtna filformat kontrolleras både via filändelse och magic bytes
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    // Magic bytes — de första bytes i filen avslöjar det verkliga filformatet,
    // oavsett vad filändelsen säger. Skyddar mot t.ex. "malware.jpg" som egentligen är en .exe
    private static readonly byte[][] ImageMagicBytes =
    [
        [0xFF, 0xD8, 0xFF],          // JPEG
        [0x89, 0x50, 0x4E, 0x47],    // PNG
        [0x52, 0x49, 0x46, 0x46],    // WEBP
    ];

    // AC3 — Max filstorlek 2 MB enligt acceptanskriteriet
    private const long MaxFileSizeBytes = 2 * 1024 * 1024;


    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllProductsAsync();
        return View(products);
    }

    public IActionResult Users()
    {
        var users = _userManager.Users.ToList();
        return View(users);
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
            // AC3 — Validera filtyp och storlek innan uppladdning
            var validationError = ValidateImageFile(imageFile);
            if (validationError != null)
            {
                // Skicka felmeddelandet via ViewData så det visas i vyn
                ViewData["imageFileError"] = validationError;
                return View(product);
            }

            // AC1 — Ladda upp bilden till Blob Storage och spara URL:en på produkten
            using var stream = imageFile.OpenReadStream();
            product.ImageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName);
            _logger.LogInformation("Image uploaded for new product {ProductName}: {ImageUrl}", product.Name, product.ImageUrl);
        }

        // ImageUrl sätts av uppladdningen — hoppa över modellvalidering för det fältet
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
            // AC3 — Validera filtyp och storlek innan uppladdning
            var validationError = ValidateImageFile(imageFile);
            if (validationError != null)
            {
                ViewData["imageFileError"] = validationError;
                return View(product);
            }

            // AC5 — Radera gamla bilden från Blob Storage innan ny laddas upp
            // Förhindrar att gamla bilder samlas i molnet och driver upp lagringskostnader
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                await _blobService.DeleteImageAsync(product.ImageUrl);
                _logger.LogInformation("Old image deleted for product {ProductId}: {ImageUrl}", id, product.ImageUrl);
            }

            // AC1 — Ladda upp ny bild och uppdatera URL:en på produkten
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

        // AC4 — Radera bilden från Blob Storage när produkten raderas
        // Livscykelhantering: undviker orphaned blobs som driver upp lagringskostnader
        if (product != null && !string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            await _blobService.DeleteImageAsync(product.ImageUrl);
            _logger.LogInformation("Image deleted from Blob Storage for product {ProductId}: {ImageUrl}", id, product.ImageUrl);
        }

        await _productService.DeleteProductAsync(id);
        _logger.LogInformation("Product deleted: {ProductId}", id);
        return RedirectToAction(nameof(Index));
    }

    // AC3 — Validering av filtyp och storlek
    // Dubbel validering: filändelse + magic bytes för att förhindra förfalskade filer
    private static string? ValidateImageFile(IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
            return "Filen är för stor. Max 2 MB tillåts.";

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            return "Otillåtet filformat. Endast .jpg, .png och .webp tillåts.";

        // Magic bytes-validering — läser filens faktiska innehåll, inte bara namnet
        using var stream = file.OpenReadStream();
        var header = new byte[4];
        var bytesRead = stream.Read(header, 0, header.Length);

        if (bytesRead < 3)
            return "Filen verkar inte vara en giltig bild.";

        var isValidImage = ImageMagicBytes.Any(magic =>
            header.Take(magic.Length).SequenceEqual(magic));

        if (!isValidImage)
            return "Filen verkar inte vara en giltig bild.";

        return null;
    }
}