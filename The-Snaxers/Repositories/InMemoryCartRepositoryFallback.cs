using TheSnaxers.Models;

namespace TheSnaxers.Repositories
{
    public class InMemoryCartRepositoryFallback : ICartRepository
    {
        private readonly Dictionary<string, ShoppingCart> _carts = new();
        public async Task<ShoppingCart> GetCartByUserIdAsync(string userId) => 
            _carts.TryGetValue(userId, out var cart) ? cart : new ShoppingCart { Id = userId, UserId = userId };
        public async Task SaveCartAsync(ShoppingCart cart) => _carts[cart.Id] = cart;
        public async Task ClearCartAsync(string userId) => _carts.Remove(userId);
    }
}