using TheSnaxers.Models;
using TheSnaxers.Repositories;

namespace TheSnaxers.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm, int? minCocoa)
    {
        return await _repo.SearchAsync(searchTerm, minCocoa);
    }
    
    public async Task<Product?> GetProductByIdAsync(string id)
    {
        // Försök konvertera strängen till int för att prata med nuvarande Repository
        if (int.TryParse(id, out int intId))
        {
            return await _repo.GetByIdAsync(intId);
        }
        return null;
    }

    public async Task UpdateProductAsync(Product product)
    {
        await _repo.UpdateAsync(product);
    }

    public async Task AddProductAsync(Product product)
    {
        await _repo.AddAsync(product);
    }

    public async Task DeleteProductAsync(string id)
    {
        // Försök konvertera strängen till int för att prata med nuvarande Repository
        if (int.TryParse(id, out int intId))
        {
            await _repo.DeleteAsync(intId);
        }
    }
}