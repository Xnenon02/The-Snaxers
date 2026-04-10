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
}