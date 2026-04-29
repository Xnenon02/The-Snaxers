using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Services; // Ny namespace-referens
using System.Security.Claims;

namespace TheSnaxers.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartSummaryViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

      public async Task<IViewComponentResult> InvokeAsync() // Ändra till async Task och InvokeAsync
{
    var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
    
    // Lägg till await här!
    var totalCount = userId != null ? await _cartService.GetTotalCountAsync(userId) : 0;
    
    return View(totalCount);
}
    }
}