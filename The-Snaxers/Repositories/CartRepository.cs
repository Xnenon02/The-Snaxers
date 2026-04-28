using System.Text.Json;
using TheSnaxers.Models;

namespace TheSnaxers.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "SnaxersCart";

        public CartRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        public void AddToCart(Product chocolate)
        {
            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == chocolate.Id);

            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = chocolate.Id,
                    ProductName = chocolate.Name, // Mappa namnet
                    Price = chocolate.Price,       // Mappa priset
                    ImageUrl = chocolate.ImageUrl, // Mappa bilden
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            SaveCart(cart);
        }

        public int RemoveFromCart(Product chocolate)
        {
            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == chocolate.Id);
            int remainingQuantity = 0;

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    remainingQuantity = cartItem.Quantity;
                }
                else
                {
                    cart.Remove(cartItem);
                }
            }

            SaveCart(cart);
            return remainingQuantity;
        }

        public List<CartItem> GetCartItems()
        {
            var sessionData = Session?.GetString(CartSessionKey);
            return sessionData == null
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }

        public void ClearCart()
        {
            Session?.Remove(CartSessionKey);
        }

        public decimal GetCartTotal()
        {
            var cart = GetCartItems();
            return cart.Sum(item => item.TotalPrice);
        }

        private void SaveCart(List<CartItem> cart)
        {
            Session?.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        public void RemoveProductCompletely(string productId)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
        }
    }
}