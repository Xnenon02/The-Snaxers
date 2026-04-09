using TheSnaxers.Models;
using TheSnaxers.Repositories;

namespace TheSnaxers.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;

    public FavoriteService(IFavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    public async Task<List<Favorite>> GetUserFavoritesAsync(string userId)
    {
        return await _favoriteRepository.GetFavoritesByUserIdAsync(userId);
    }

    public async Task AddToFavoritesAsync(string userId, int productId)
    {
        var exists = await _favoriteRepository.ExistsAsync(userId, productId);
        if (!exists)
        {
            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId
            };
            await _favoriteRepository.AddAsync(favorite);
        }
    }

    public async Task RemoveFromFavoritesAsync(string userId, int productId)
    {
        await _favoriteRepository.RemoveAsync(userId, productId);
    }
}