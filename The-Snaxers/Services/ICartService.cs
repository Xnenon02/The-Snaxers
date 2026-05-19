using TheSnaxers.Models;

public interface ICartService
{
    Task<int> GetTotalCountAsync(string userId);
    Task<ShoppingCart> GetCartByUserIdAsync(string userId); // Tillagd!
    Task AddToCartAsync(string userId, CartItem newItem);
    Task RemoveFromCartAsync(string userId, string productId); // Tillagd!
    Task ClearProductFromCartAsync(string userId, string productId); // Tillagd!
}