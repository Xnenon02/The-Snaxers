using TheSnaxers.Models;

public interface ICartRepository
{
    void AddToCart(Product chocolate);
    int RemoveFromCart(Product chocolate); // Returnerar antal kvar av varan
    List<CartItem> GetCartItems();
    void ClearCart();
    decimal GetCartTotal();
    void RemoveProductCompletely(string productId);
}