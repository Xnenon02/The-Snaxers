using TheSnaxers.Models;

namespace TheSnaxers.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<List<Product>> SearchProductsAsync(string searchTerm, int? minCocoa);
}