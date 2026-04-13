using TheSnaxers.Models;

namespace TheSnaxers.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> SearchAsync(string searchTerm, int? minCocoa);
    Task AddAsync(Product product);
    Task<Product?> GetByIdAsync(int id);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}