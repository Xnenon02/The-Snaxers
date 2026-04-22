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

    public async Task<List<Product>> GetAllProductsAsync() =>
        await _repo.GetAllAsync();

    public async Task<List<Product>> SearchProductsAsync(string searchTerm, int? minCocoa) =>
        await _repo.SearchAsync(searchTerm, minCocoa);

    public async Task<Product?> GetProductByIdAsync(string id) =>
        await _repo.GetByIdAsync(id);

    public async Task AddProductAsync(Product product) =>
        await _repo.AddAsync(product);

    public async Task UpdateProductAsync(Product product, string originalCategory) =>
        await _repo.UpdateAsync(product, originalCategory);

    public async Task DeleteProductAsync(string id, string category) =>
        await _repo.DeleteAsync(id, category);
}