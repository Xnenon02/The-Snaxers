using Microsoft.EntityFrameworkCore;
using TheSnaxers.Data;
using TheSnaxers.Models;

namespace TheSnaxers.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly ApplicationDbContext _db;

    public FavoriteRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Favorite>> GetFavoritesByUserIdAsync(string userId)
    {
        return await _db.Favorites
            .Include(f => f.Product)
            .Where(f => f.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string userId, int productId)
    {
        return await _db.Favorites
            .AnyAsync(f => f.UserId == userId && f.ProductId == productId);
    }

    public async Task AddAsync(Favorite favorite)
    {
        _db.Favorites.Add(favorite);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(string userId, int productId)
    {
        var favorite = await _db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

        if (favorite != null)
        {
            _db.Favorites.Remove(favorite);
            await _db.SaveChangesAsync();
        }
    }
}