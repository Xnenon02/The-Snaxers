using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Services; 
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace TheSnaxers.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
{
    private readonly ICartService _cartService;
    private readonly IMemoryCache _cache; 

    public CartSummaryViewComponent(ICartService cartService, IMemoryCache cache)
    {
        _cartService = cartService;
        _cache = cache;
    }

    public async Task<IViewComponentResult> InvokeAsync()
{
    // I ViewComponents måste vi skriva HttpContext.User
    var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    
    if (string.IsNullOrEmpty(userId))
    {
        return View(0); // Om ingen är inloggad, visa 0
    }

    string cacheKey = $"cart_count_{userId}";

    if (!_cache.TryGetValue(cacheKey, out int count))
    {
        count = await _cartService.GetTotalCountAsync(userId);
        _cache.Set(cacheKey, count, TimeSpan.FromMinutes(5));
    }

    return View(count);
}
}
}