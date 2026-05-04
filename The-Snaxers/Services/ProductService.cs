using TheSnaxers.Models;
using TheSnaxers.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace TheSnaxers.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IMemoryCache _cache;

    // Cache key and duration for the full product list
    private const string AllProductsCacheKey = "all_products";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ProductService(IProductRepository repo, IMemoryCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        // Return cached product list if available, avoiding repeated Cosmos DB round-trips
        if (_cache.TryGetValue(AllProductsCacheKey, out List<Product>? cached) && cached != null)
            return cached;

        var products = await _repo.GetAllAsync();

        // Cache the result to speed up subsequent page loads
        _cache.Set(AllProductsCacheKey, products, CacheDuration);

        return products;
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm, int? minCocoa) =>
        await _repo.SearchAsync(searchTerm, minCocoa);

    public async Task<Product?> GetProductByIdAsync(string id) =>
        await _repo.GetByIdAsync(id);

    public async Task AddProductAsync(Product product)
    {
        await _repo.AddAsync(product);
        // Invalidate cache so the new product appears immediately
        _cache.Remove(AllProductsCacheKey);
    }

    public async Task UpdateProductAsync(Product product, string originalCategory)
    {
        await _repo.UpdateAsync(product, originalCategory);
        // Invalidate cache so the updated product appears immediately
        _cache.Remove(AllProductsCacheKey);
    }

    public async Task DeleteProductAsync(string id, string category)
    {
        await _repo.DeleteAsync(id, category);
        // Invalidate cache so the deleted product disappears immediately
        _cache.Remove(AllProductsCacheKey);
    }
}