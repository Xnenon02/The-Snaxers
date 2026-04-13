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
    public async Task AddAsync(Product product)
{
    await _db.Products.AddAsync(product);
    await _db.SaveChangesAsync();
}

    public async Task<List<Product>> GetAllAsync()
    {
        return await _db.Products.ToListAsync();
    }

   public async Task<List<Product>> SearchAsync(string searchTerm, int? minCocoa)
    {
    var query = _db.Products.AsQueryable();

    if (!string.IsNullOrWhiteSpace(searchTerm))
        query = query.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));

    if (minCocoa.HasValue)
        query = query.Where(p => p.CocoaPercentage >= minCocoa.Value);

    return await query.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
{
    return await _db.Products.FindAsync(id);
}

public async Task UpdateAsync(Product product)
{
    _db.Products.Update(product);
    await _db.SaveChangesAsync();
}

   public async Task DeleteAsync(int id)
{
    // Vi ändrar _context till _db här:
    var product = await _db.Products.FindAsync(id); 
    if (product != null)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }
}
}
