using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TheSnaxers.Data;
using TheSnaxers.Models;
using Microsoft.EntityFrameworkCore;

namespace TheSnaxers.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public ProductController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _db.Products.ToListAsync();

        var userId = _userManager.GetUserId(User);
        var favoriteIds = userId != null
            ? await _db.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.ProductId)
                .ToListAsync()
            : new List<int>();

        ViewBag.FavoriteIds = favoriteIds;
        return View(products);
    }
}