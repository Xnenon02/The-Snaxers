using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Repositories;
using TheSnaxers.Services; // Se till att du har denna!
using TheSnaxers.Models;
using System.Security.Claims;
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

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException(
                "UserId saknas trots autentisering. Kontrollera att [Authorize] är satt och att " +
                "ClaimTypes.NameIdentifier (NameIdentifier-claim) populeras korrekt i login-logiken.");

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

                // 2. Matcha ihop dem (Berika korgen) — dictionary för O(1)-lookup istället för O(n) per item
                var productMap = allProducts.ToDictionary(p => p.Id);
                foreach (var item in cart.Items)
                {
                    if (productMap.TryGetValue(item.ProductId, out var product))
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

        // [ValidateAntiForgeryToken] is intentionally removed from AddToCart.
        // GET requests (post-login redirects from Identity) carry no antiforgery token,
        // so the attribute would cause HTTP 400 on every login redirect.
        // Instead, antiforgery is validated manually for POST requests only.
        // The unauthenticated cart flow works as follows:
        //   1. User clicks "Lägg i korgen" → redirected to login with returnUrl=/Cart/AddToCart?productId=...
        //   2. After login, Identity redirects back via GET → product is added to cart automatically
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, string returnUrl)
        {
            // GET requests after login redirect — add product to cart and redirect
            if (HttpMethods.IsGet(Request.Method))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Index", "Chocolate");

                if (!string.IsNullOrEmpty(productId))
                {
                    var chocolate = await _chocolateRepository.GetByIdAsync(productId);
                    if (chocolate != null)
                    {
                        await _cartService.AddToCartAsync(userId, new CartItem
                        {
                            ProductId = productId,
                            ProductName = chocolate.Name,
                            Price = chocolate.Price,
                            Quantity = 1
                        });
                        _cache.Remove($"cart_count_{userId}");
                        TempData["SuccessMessage"] = "Produkten har lagts i din varukorg! 🍫";
                    }
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Chocolate");
            }

            // POST requests — validate antiforgery token manually
            try
            {
                var antiforgery = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
                await antiforgery.ValidateRequestAsync(HttpContext);
            }
            catch
            {
                return BadRequest();
            }

            var chocolate2 = await _chocolateRepository.GetByIdAsync(productId);

            if (chocolate2 != null)
            {
                // Här skapar vi variabeln istället för att hårdkoda '1' längre ner
                int quantity = 1;

                await _cartService.AddToCartAsync(UserId, new CartItem
                {
                    ProductId = productId,
                    ProductName = chocolate2.Name,
                    Price = chocolate2.Price,
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