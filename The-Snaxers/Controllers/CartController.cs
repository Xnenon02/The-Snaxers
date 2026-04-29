using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Repositories;
using TheSnaxers.Services; // Se till att du har denna!
using TheSnaxers.Models;
using System.Security.Claims;

namespace TheSnaxers.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _chocolateRepository;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        // FIX 1: Se till att namnen matchar i konstruktorn (_cartService, inte _cartRepository)
        public CartController(IProductRepository chocolateRepository, ICartService cartService, IProductService productService)
        {
            _chocolateRepository = chocolateRepository;
            _cartService = cartService;
            _productService = productService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Visar själva varukorgs-sidan
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            decimal runningTotal = 0;

            // För varje sak i korgen, hämta den riktiga produkt-infon från produktservicen
            foreach (var item in cart.Items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    // Vi "lånar" informationen från produkten och lägger på korg-objektet
                    item.ProductName = product.Name;
                    item.Price = product.Price;
                    item.ImageUrl = product.ImageUrl; 
                    runningTotal += (item.Price * item.Quantity);
                }
            }

            ViewBag.GrandTotal = runningTotal;

            return View(cart.Items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(string productId, string returnUrl)
        {
            var chocolate = await _chocolateRepository.GetByIdAsync(productId);

            if (chocolate != null)
            {
                // FIX 3: Skicka med UserId och skapa ett CartItem
                await _cartService.AddToCartAsync(UserId, new CartItem
                {
                    ProductId = productId,
                    ProductName = chocolate.Name, // Om din modell har namn
                    Price = chocolate.Price,
                    Quantity = 1
                });
            }

            // Om vi har en returnUrl, skicka användaren tillbaka dit
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProductCompletely(string productId)
        {
            // FIX 5: Gör även denna asynkron i servicen!
            await _cartService.ClearProductFromCartAsync(UserId, productId);
            return RedirectToAction("Index");
        }
    }
}