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

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly byte[][] ImageMagicBytes =
    [
        [0xFF, 0xD8, 0xFF],
        [0x89, 0x50, 0x4E, 0x47],
        [0x52, 0x49, 0x46, 0x46],
    ];

    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllProductsAsync();
        return View(products);
    }

    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.ToList();
        var userRoles = new Dictionary<string, IList<string>>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles[user.Id] = roles;
        }

        ViewBag.UserRoles = userRoles;
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
        _logger.LogInformation("Admin attempt to create product: {ProductName}", product.Name);

        if (imageFile != null && imageFile.Length > 0)
        {
            var validationError = ValidateImageFile(imageFile);
            if (validationError != null)
            {
                ViewData["imageFileError"] = validationError;
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
            _logger.LogInformation("Successfully added {ProductName} to the database.", product.Name);
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Validation failed for creating product: {ProductName}", product.Name);
        return View(product);
    }

    public async Task<IActionResult> Edit(string id)
    {
        // FIX: Konvertera int id till string
        var product = await _productService.GetProductByIdAsync(id.ToString());
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Product product, IFormFile? imageFile)
    {
        _logger.LogInformation("Admin attempt to edit product ID: {ProductId}", id);

        if (id != product.Id)
        {
            _logger.LogError("ID mismatch during edit: URL ID {UrlId} != Model ID {ModelId}", id, product.Id);
            return BadRequest("ID mismatch - Manipulation detekterad.");
        }

        if (imageFile != null && imageFile.Length > 0)
        {
            var validationError = ValidateImageFile(imageFile);
            if (validationError != null)
            {
                ViewData["imageFileError"] = validationError;
                return View(product);
            }

            // AC5 — Radera gamla bilden från Blob Storage innan ny laddas upp
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
        ModelState.Remove("ImageUrl");

        if (ModelState.IsValid)
        {
            // FIX: Vi utgår från att interfacet nu tar Product (vilket det gör)
            await _productService.UpdateProductAsync(product);
            _logger.LogInformation("Successfully updated product ID: {ProductId}", id);
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Validation failed for editing product ID: {ProductId}", id);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Delete: Product {ProductId} not found.", id);
            return RedirectToAction(nameof(Index));
        }

        // AC4 — Radera bilden från Blob Storage när produkten raderas
        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            await _blobService.DeleteImageAsync(product.ImageUrl);
            _logger.LogInformation("Image deleted from Blob Storage for product {ProductId}: {ImageUrl}", id, product.ImageUrl);
        }

        // Skickar med Category som partition key — eliminerar dubbelanropet i repository (tech debt #2)
        await _productService.DeleteProductAsync(id, product.Category);
        _logger.LogInformation("Product deleted: {ProductId}", id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user != null)
        {
            var result = await _userManager.AddToRoleAsync(user, "Admin");

            if (result.Succeeded)
            {
                _logger.LogInformation("Användare {Email} har blivit befordrad till Admin.", user.Email);
            }
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound();

        if (user.Email == User.Identity?.Name)
        {
            TempData["Error"] = "Du kan inte ta bort dina egna admin-rättigheter!";
            return RedirectToAction(nameof(Users));
        }

        await _userManager.RemoveFromRoleAsync(user, "Admin");
        _logger.LogWarning("Admin-rättigheter borttagna för {Email}.", user.Email);

        return RedirectToAction(nameof(Users));
    }

    // AC3 — Validering av filtyp och storlek
    private static string? ValidateImageFile(IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
            return "Filen är för stor. Max 2 MB tillåts.";

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            return "Otillåtet filformat. Endast .jpg, .png och .webp tillåts.";

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