using TheSnaxers.Models;

namespace TheSnaxers.Services;

public interface IFavoriteService
{
    Task<List<Favorite>> GetUserFavoritesAsync(string userId);
    Task AddToFavoritesAsync(string userId, int productId);
    Task RemoveFromFavoritesAsync(string userId, int productId);
}