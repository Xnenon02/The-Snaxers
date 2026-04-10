using Microsoft.EntityFrameworkCore;
using TheSnaxers.Data;
using TheSnaxers.Models;

namespace TheSnaxers.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _db.Products.ToListAsync();
    }

    public async Task<List<Product>> SearchAsync(string searchTerm, int? minCocoa)
    {
        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm));

        if (minCocoa.HasValue)
            query = query.Where(p => p.CocoaPercentage >= minCocoa.Value);

        return await query.ToListAsync();
    }
}