using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheSnaxers.Data;
using TheSnaxers.Models;

namespace TheSnaxers.Controllers;

// [Authorize]
public class FavoriteController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public FavoriteController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var favorites = await _db.Favorites
            .Include(f => f.Product)
            .Where(f => f.UserId == userId)
            .ToListAsync();
        return View(favorites);
    }

    [HttpPost]
public async Task<IActionResult> Add(int productId)
{
    var userId = _userManager.GetUserId(User);

    if (userId == null)
        return RedirectToAction("Index", "Product");

    var exists = await _db.Favorites
        .AnyAsync(f => f.UserId == userId && f.ProductId == productId);

    if (!exists)
    {
        _db.Favorites.Add(new Favorite
        {
            UserId = userId,
            ProductId = productId
        });
        await _db.SaveChangesAsync();
    }

    return RedirectToAction("Index", "Product");
}

    [HttpPost]
    public async Task<IActionResult> Remove(int productId)
    {
        var userId = _userManager.GetUserId(User);
        var favorite = await _db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
        if (favorite != null)
        {
            _db.Favorites.Remove(favorite);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}