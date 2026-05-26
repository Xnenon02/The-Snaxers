using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TheSnaxers.Services;
using TheSnaxers.Models;
using Microsoft.AspNetCore.Identity;

namespace TheSnaxers.Controllers;

[Authorize(Roles = "Admin")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)] // Hindrar cachning av känsliga formulär och data
public class AdminChocolateController : Controller
{
    private readonly IProductService _productService;
    private readonly IBlobService _blobService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AdminChocolateController> _logger;
    private readonly IImageValidationService _imageValidationService; 

    public AdminChocolateController(
        IProductService productService, 
        IBlobService blobService, 
        UserManager<IdentityUser> userManager,
        ILogger<AdminChocolateController> logger,
        IImageValidationService imageValidationService)
    {
        _productService = productService;
        _blobService = blobService;
        _userManager = userManager;
        _logger = logger;
        _imageValidationService = imageValidationService;
    }

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
    return View(new CreateProductViewModel());
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateProductViewModel model, IFormFile? imageFile)
{
    _logger.LogInformation("Admin attempt to create product: {ProductName}", model.Name);

    string imageUrl = string.Empty;

    if (imageFile != null && imageFile.Length > 0)
    {
        // Använd vår nya tjänst istället för den gamla interna metoden!
        var validationError = _imageValidationService.ValidateImageFile(imageFile);
        if (validationError != null)
        {
            ViewData["imageFileError"] = validationError;
            return View(model);
        }

        using var stream = imageFile.OpenReadStream();
        imageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName);
        _logger.LogInformation("Image uploaded for new product {ProductName}: {ImageUrl}", model.Name, imageUrl);
    }

    if (!ModelState.IsValid)
    {
        _logger.LogWarning("Validation failed for creating product: {ProductName}", model.Name);
        return View(model);
    }

    // Explicit mappning från ViewModel till säker Domänmodell
    var product = new Product
    {
        Id = Guid.NewGuid().ToString(),
    Name = model.Name,
    Brand = model.Brand,
    Price = model.Price,
    CocoaPercentage = model.CocoaPercentage,
    Weight = model.Weight, 
    StockLevel = model.StockLevel,
    Country = model.Country,
    Category = model.Category,
    Description = model.Description,
    ImageUrl = imageUrl
    };

    await _productService.AddProductAsync(product);
    _logger.LogInformation("Successfully added {ProductName} to the database.", product.Name);
    return RedirectToAction(nameof(Index));
}

public async Task<IActionResult> Edit(string id)
{
    var product = await _productService.GetProductByIdAsync(id);
    if (product == null) return NotFound();

    // Mappa ALLA egenskaper från domänmodellen till din ViewModel inför visning i vyn
    var viewModel = new EditProductViewModel
    {
        Id = product.Id,
        Name = product.Name,
        Brand = product.Brand,             
        Price = product.Price,
        CocoaPercentage = product.CocoaPercentage, 
        Weight = product.Weight,           
        StockLevel = product.StockLevel,
        Country = product.Country,        
        Category = product.Category,
        Description = product.Description,
        ImageUrl = product.ImageUrl
    };

    return View(viewModel);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(string id, EditProductViewModel model, IFormFile? imageFile, string originalCategory = "")
{
    _logger.LogInformation("Admin attempt to edit product ID: {ProductId}", id);
    model.Id = id;

    // Hämta produkten för att behålla nuvarande bild-URL om ingen ny laddas upp
    var existingProduct = await _productService.GetProductByIdAsync(id);
    if (existingProduct == null) return NotFound();
    
    string currentImageUrl = existingProduct.ImageUrl ?? string.Empty;

    if (imageFile != null && imageFile.Length > 0)
    {
        var validationError = _imageValidationService.ValidateImageFile(imageFile);
        if (validationError != null)
        {
            ViewData["imageFileError"] = validationError;
            model.ImageUrl = currentImageUrl;
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(currentImageUrl))
        {
            await _blobService.DeleteImageAsync(currentImageUrl);
            _logger.LogInformation("Old image deleted for product {ProductId}: {ImageUrl}", id, currentImageUrl);
        }

        using var stream = imageFile.OpenReadStream();
        currentImageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName);
        _logger.LogInformation("New image uploaded for product {ProductId}: {ImageUrl}", id, currentImageUrl);
    }

    if (!ModelState.IsValid)
    {
        model.ImageUrl = currentImageUrl;
        return View(model);
    }

    // Bygg domänobjektet utifrån validerad data
    var product = new Product
    {
        Id = id,
        Name = model.Name,
        Brand = model.Brand,
        CocoaPercentage = model.CocoaPercentage,
        Weight = model.Weight,
        Country = model.Country,
        Description = model.Description,
        Price = model.Price,
        Category = model.Category,
        StockLevel = model.StockLevel,
        ImageUrl = currentImageUrl
    };
// COMMENT: Direkt uppdatering av StockLevel via ProductService/Repository är avsedd för 
    // administrativa justeringar (t.ex. manuell inventering eller korrigering av lagersaldo).
    // För automatiska lagertransaktioner vid kundköp i kassan används InventoryService (DeductStockAsync).
    await _productService.UpdateProductAsync(product, originalCategory);
    _logger.LogInformation("Successfully updated product ID: {ProductId}", id);
    return RedirectToAction(nameof(Index));
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
}