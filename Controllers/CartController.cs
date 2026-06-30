using ApnaKrishi.Data;
using ApnaKrishi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApnaKrishi.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                    .ThenInclude(p => p!.Category)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            ViewBag.GrandTotal = cartItems.Sum(c => c.TotalPrice);
            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            var product = await _context.Products.FindAsync(productId);

            if (product == null || product.Stock < quantity)
            {
                TempData["Error"] = "Product not available.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                if (cartItem.Quantity > product.Stock)
                    cartItem.Quantity = product.Stock;
            }
            else
            {
                _context.Carts.Add(new Cart
                {
                    UserId = userId!,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product added to cart.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.UserId == userId);

            if (item == null) return NotFound();

            if (quantity <= 0)
                _context.Carts.Remove(item);
            else
            {
                item.Quantity = Math.Min(quantity, item.Product!.Stock);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartId)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.Carts
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.UserId == userId);

            if (item != null)
            {
                _context.Carts.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = _userManager.GetUserId(User);
            var count = await _context.Carts.Where(c => c.UserId == userId).SumAsync(c => c.Quantity);
            return Json(count);
        }
    }
}
