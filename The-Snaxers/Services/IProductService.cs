using TheSnaxers.Models;

namespace TheSnaxers.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<List<Product>> SearchProductsAsync(string searchTerm, int? minCocoa);
    Task<Product?> GetProductByIdAsync(string id);
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(string id);
}