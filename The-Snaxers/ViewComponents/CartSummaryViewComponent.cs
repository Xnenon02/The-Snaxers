using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Repositories;

namespace TheSnaxers.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ICartRepository _cartRepository;

        public CartSummaryViewComponent(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public IViewComponentResult Invoke()
        {
            var items = _cartRepository.GetCartItems();
            var totalCount = items.Sum(i => i.Quantity);
            return View(totalCount);
        }
    }
}