using TheSnaxers.Models;

namespace TheSnaxers.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<List<Product>> SearchProductsAsync(string searchTerm, int? minCocoa);
    Task AddProductAsync(Product product);
    
    // Ändrad från int till string för att matcha nya ID-standarden
    Task<Product?> GetProductByIdAsync(string id); 
    
    Task UpdateProductAsync(Product product);   // Spara ändringar
    
    // Ändrad från int till string
    Task DeleteProductAsync(string id);
}