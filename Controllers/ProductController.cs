using ApnaKrishi.Data;
using ApnaKrishi.Models;
using ApnaKrishi.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApnaKrishi.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId,
            decimal? minPrice, decimal? maxPrice, int page = 1)
        {
            int pageSize = 12;

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.ProductName.Contains(search) || p.Description!.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var total = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new ProductListViewModel
            {
                Products = products,
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                SearchTerm = search,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null) return NotFound();

            // Related products
            ViewBag.RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id && p.IsActive)
                .Take(4).ToListAsync();

            return View(product);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string? comment)
        {
            var userId = _userManager.GetUserId(User);

            // Check if already reviewed
            var existing = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            if (existing != null)
            {
                existing.Rating = rating;
                existing.Comment = comment;
                existing.ReviewDate = DateTime.Now;
            }
            else
            {
                _context.Reviews.Add(new Review
                {
                    ProductId = productId,
                    UserId = userId!,
                    Rating = rating,
                    Comment = comment,
                    ReviewDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Review submitted successfully.";
            return RedirectToAction("Details", new { id = productId });
        }
    }
}
