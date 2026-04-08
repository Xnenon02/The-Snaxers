using Microsoft.AspNetCore.Mvc;
using TheSnaxers.Data;
using TheSnaxers.Models;
using Microsoft.EntityFrameworkCore;

namespace TheSnaxers.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _db.Products.ToListAsync();
        return View(products);
    }
}