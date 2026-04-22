using Microsoft.AspNetCore.Mvc;
using TheSnaxers.DTOs;
using TheSnaxers.Services;
using TheSnaxers.Filters;

namespace TheSnaxers.Controllers;

// ===================================================
// REST API för produkter — körs parallellt med MVC
// OpenAPI-dokumentation: /openapi/v1.json
// Scalar UI: /scalar/v1
// Skyddas av API-nyckel via X-Api-Key header (ApiKeyFilter)
// ===================================================
[ApiController]
[Route("api/products")]
[ServiceFilter(typeof(ApiKeyFilter))]
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

    // GET /api/products
    /// <summary>Returns all chocolate products</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("API: Fetching all products");
        var products = await _productService.GetAllProductsAsync();

        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Brand = p.Brand,
            CocoaPercentage = p.CocoaPercentage,
            Country = p.Country,
            Description = p.Description,
            Price = p.Price,
            Category = p.Category,
            ImageUrl = p.ImageUrl
        });

        return Ok(dtos);
    }

    // GET /api/products/{id}
    /// <summary>Returns a single chocolate product by ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("API: Fetching product {ProductId}", id);
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("API: Product {ProductId} not found", id);
            return NotFound(new { message = $"Product with id {id} not found" });
        }

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Brand = product.Brand,
            CocoaPercentage = product.CocoaPercentage,
            Country = product.Country,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            ImageUrl = product.ImageUrl
        };

        return Ok(dto);
    }

    // GET /api/products/search
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

        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Brand = p.Brand,
            CocoaPercentage = p.CocoaPercentage,
            Country = p.Country,
            Description = p.Description,
            Price = p.Price,
            Category = p.Category,
            ImageUrl = p.ImageUrl
        });

        return Ok(dtos);
    }
}