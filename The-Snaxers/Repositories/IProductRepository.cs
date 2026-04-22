using TheSnaxers.Models;

namespace TheSnaxers.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> SearchAsync(string searchTerm, int? minCocoa);
    Task<Product?> GetByIdAsync(string id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product, string originalCategory);
    Task DeleteAsync(string id, string category);
}