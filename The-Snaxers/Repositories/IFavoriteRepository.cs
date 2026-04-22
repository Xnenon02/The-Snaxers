using TheSnaxers.Models;

namespace TheSnaxers.Repositories;

public interface IFavoriteRepository
{
    Task<List<Favorite>> GetFavoritesByUserIdAsync(string userId);
    Task<bool> ExistsAsync(string userId, string productId);
    Task AddAsync(Favorite favorite);
    Task RemoveAsync(string userId, string productId);
}