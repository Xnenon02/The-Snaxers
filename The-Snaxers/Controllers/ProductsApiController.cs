using Microsoft.AspNetCore.Mvc;
using TheSnaxers.DTOs;
using TheSnaxers.Services;
using TheSnaxers.Models;
using Microsoft.AspNetCore.Authorization;

namespace TheSnaxers.Controllers;

// ===================================================
// REST API för produkter — körs parallellt med MVC
// OpenAPI-dokumentation: /openapi/v1.json
// Scalar UI: /scalar/v1
// 🔒 Skyddas fullt ut med JWT Bearer Tokens!
// ===================================================
[ApiController]
[Route("api/v1/products")]
[Authorize(AuthenticationSchemes = "Bearer")] // 🔒 Ändrat hit! Skyddar NU ALLA endpoints i denna controller med JWT
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)] 
public class ProductsApiController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsApiController> _logger;

    public ProductsApiController(
        IProductService productService,
        ILogger<ProductsApiController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    // GET /api/v1/products
    /// <summary>Returns all chocolate products</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("API: Fetching all products");
        var products = await _productService.GetAllProductsAsync();
        return Ok(products.Select(MapToDto));
    }

    // GET /api/v1/products/{id}
    /// <summary>Returns a single chocolate product by ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(string id) 
    {
        _logger.LogInformation("API: Fetching product {ProductId}", id);
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("API: Product {ProductId} not found", id);
            return NotFound(new { message = $"Product with id {id} not found" });
        }

        return Ok(MapToDto(product));
    }

    // GET /api/v1/products/search
    /// <summary>Search and filter products by name and cocoa percentage</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] int? minCocoa)
    {
        _logger.LogInformation("API: Searching products with term '{SearchTerm}' and minCocoa {MinCocoa}", searchTerm, minCocoa);
        var products = await _productService.SearchProductsAsync(searchTerm ?? "", minCocoa);
        return Ok(products.Select(MapToDto));
    }

    // Maps a Product domain model to a ProductDto
    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id.ToString(), 
        Name = p.Name,
        Brand = p.Brand,
        CocoaPercentage = p.CocoaPercentage,
        Country = p.Country,
        Description = p.Description,
        Price = p.Price,
        Category = p.Category,
        ImageUrl = p.ImageUrl
    };
}