using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Repositories;

namespace TheSnaxers.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _chocolateRepository;
        private readonly ICartRepository _cartRepository;

        public CartController(IProductRepository chocolateRepository, ICartRepository cartRepository)
        {
            _chocolateRepository = chocolateRepository;
            _cartRepository = cartRepository;
        }

        // Visar själva varukorgs-sidan
        public IActionResult Index()
        {
            var items = _cartRepository.GetCartItems();
            var total = _cartRepository.GetCartTotal();

            ViewBag.CartTotal = total;
            return View(items);
        }

        // Action för att lägga till choklad
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId)
        {
            var chocolate = await _chocolateRepository.GetByIdAsync(productId);

            if (chocolate != null)
            {
                _cartRepository.AddToCart(chocolate);
            }

            // Efter man lagt till vill man oftast komma tillbaka till där man var
            // eller direkt till varukorgen. Vi kör varukorgen för nu:
            return RedirectToAction("Index");
        }

        // Action för att ta bort/minska antal
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            var chocolate = await _chocolateRepository.GetByIdAsync(productId);

            if (chocolate != null)
            {
                _cartRepository.RemoveFromCart(chocolate);
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult RemoveProductCompletely(string productId)
        {
            _cartRepository.RemoveProductCompletely(productId);
            return RedirectToAction("Index");
        }
    }
}