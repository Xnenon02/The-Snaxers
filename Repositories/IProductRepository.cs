using TheSnaxers.Models;

namespace TheSnaxers.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> SearchAsync(string searchTerm, int? minCocoa);
}