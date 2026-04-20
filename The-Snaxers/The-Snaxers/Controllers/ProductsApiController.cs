using Microsoft.AspNetCore.Mvc;
using TheSnaxers.DTOs;
using TheSnaxers.Services;

namespace TheSnaxers.Controllers;

// ===================================================
// REST API för produkter — körs parallellt med MVC
// Swagger-dokumentation: /swagger
// Skyddas av API-nyckel via X-Api-Key header
// ===================================================
[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsApiController> _logger;
    private readonly IConfiguration _configuration;

    public ProductsApiController(
        IProductService productService,
        ILogger<ProductsApiController> logger,
        IConfiguration configuration)
    {
        _productService = productService;
        _logger = logger;
        _configuration = configuration;
    }

    // GET /api/products
    /// <summary>Returns all chocolate products</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll([FromHeader(Name = "X-Api-Key")] string? apiKey)
    {
        if (!IsValidApiKey(apiKey))
        {
            _logger.LogWarning("Unauthorized API access attempt to GET /api/products");
            return Unauthorized(new { message = "Invalid or missing API key" });
        }

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
    public async Task<IActionResult> GetById(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        int id)
    {
        if (!IsValidApiKey(apiKey))
        {
            _logger.LogWarning("Unauthorized API access attempt to GET /api/products/{Id}", id);
            return Unauthorized(new { message = "Invalid or missing API key" });
        }

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
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromQuery] string? searchTerm,
        [FromQuery] int? minCocoa)
    {
        if (!IsValidApiKey(apiKey))
        {
            _logger.LogWarning("Unauthorized API access attempt to GET /api/products/search");
            return Unauthorized(new { message = "Invalid or missing API key" });
        }

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

    private bool IsValidApiKey(string? apiKey)
    {
        var validKey = _configuration["ApiKey"];
        return !string.IsNullOrWhiteSpace(apiKey) && apiKey == validKey;
    }
}