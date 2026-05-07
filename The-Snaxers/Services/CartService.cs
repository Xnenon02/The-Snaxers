namespace TheSnaxers.Services;
using TheSnaxers.Models;
using TheSnaxers.Repositories;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<ShoppingCart> GetCartByUserIdAsync(string userId)
    {
        return await _cartRepository.GetCartByUserIdAsync(userId);
    }

    public async Task<int> GetTotalCountAsync(string userId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        return cart?.Items.Sum(item => item.Quantity) ?? 0;
    }

    public async Task AddToCartAsync(string userId, CartItem newItem)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == newItem.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += newItem.Quantity;
        }
        else
        {
            cart.Items.Add(newItem);
        }
        await _cartRepository.SaveCartAsync(cart);
    }

    // Hanitas fix: Minska antal eller ta bort om det är sista
    public async Task RemoveFromCartAsync(string userId, string productId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            if (existingItem.Quantity > 1)
            {
                existingItem.Quantity--;
            }
            else
            {
                cart.Items.Remove(existingItem);
            }
            await _cartRepository.SaveCartAsync(cart);
        }
    }

    // Ta bort hela raden (t.ex. om man klickar på soptunnan)
    public async Task ClearProductFromCartAsync(string userId, string productId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            cart.Items.Remove(existingItem);
            await _cartRepository.SaveCartAsync(cart);
        }
    }
}