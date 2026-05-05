using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Repositories;
using TheSnaxers.Services; // Se till att du har denna!
using TheSnaxers.Models;
using System.Security.Claims;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Caching.Memory;

namespace TheSnaxers.Controllers
{
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)] // Förhindrar cachning av varukorgsdata och tokens
    public class CartController : Controller
    {
        private readonly IProductRepository _chocolateRepository;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly IMemoryCache _cache;

        // FIX 1: Se till att namnen matchar i konstruktorn (_cartService, inte _cartRepository)
        public CartController(IProductRepository chocolateRepository, ICartService cartService, IProductService productService, IMemoryCache cache)
        {
            _chocolateRepository = chocolateRepository;
            _cartService = cartService;
            _productService = productService;
            _cache = cache;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Visar själva varukorgs-sidan
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart.Items.Any())
            {
                // 1. Hämta ALLA produkter som finns i korgen i ett svep
                var allProducts = await _productService.GetAllProductsAsync(); // Eller en metod som tar en lista med ID:n

                // 2. Matcha ihop dem (Berika korgen)
                foreach (var item in cart.Items)
                {
                    var product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        item.ProductName = product.Name;
                        item.Price = product.Price;
                        item.ImageUrl = product.ImageUrl;
                    }
                }
            }

            ViewBag.GrandTotal = cart.Items.Sum(x => x.Price * x.Quantity).ToString("N2");
            return View(cart.Items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(string productId, string returnUrl)
        {
            var chocolate = await _chocolateRepository.GetByIdAsync(productId);

            if (chocolate != null)
            {
                // Här skapar vi variabeln istället för att hårdkoda '1' längre ner
                int quantity = 1;

                await _cartService.AddToCartAsync(UserId, new CartItem
                {
                    ProductId = productId,
                    ProductName = chocolate.Name,
                    Price = chocolate.Price,
                    Quantity = quantity // Använd variabeln här!

                });
                _cache.Remove($"cart_count_{UserId}");
            }
            TempData["SuccessMessage"] = "Produkten har lagts i din varukorg! 🍫";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            // FIX 4: Använd servicens asynkrona metod
            await _cartService.RemoveFromCartAsync(UserId, productId);
            _cache.Remove($"cart_count_{UserId}");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProductCompletely(string productId)
        {
            // FIX 5: Gör även denna asynkron i servicen!
            await _cartService.ClearProductFromCartAsync(UserId, productId);
            _cache.Remove($"cart_count_{UserId}");
            return RedirectToAction("Index");
        }
    }
}