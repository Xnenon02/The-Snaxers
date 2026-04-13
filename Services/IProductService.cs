using TheSnaxers.Models;

namespace TheSnaxers.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<List<Product>> SearchProductsAsync(string searchTerm, int? minCocoa);
    Task AddProductAsync(Product product);
    Task<Product?> GetProductByIdAsync(int id); // Hämta en specifik
    Task UpdateProductAsync(Product product);   // Spara ändringar
    Task DeleteProductAsync(int id);
}