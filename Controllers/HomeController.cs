using ApnaKrishi.Data;
using ApnaKrishi.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApnaKrishi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel
            {
                FeaturedProducts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsFeatured && p.IsActive && p.Stock > 0)
                    .Take(8).ToListAsync(),

                BestSellers = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsBestSeller && p.IsActive && p.Stock > 0)
                    .Take(6).ToListAsync(),

                NewArrivals = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsNewArrival && p.IsActive && p.Stock > 0)
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(6).ToListAsync(),

                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),

                Fertilizers = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Category!.CategoryName == "Fertilizers" && p.IsActive)
                    .Take(4).ToListAsync(),

                Seeds = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Category!.CategoryName == "Seeds" && p.IsActive)
                    .Take(4).ToListAsync(),

                Pesticides = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Category!.CategoryName == "Pesticides" && p.IsActive)
                    .Take(4).ToListAsync(),

                FarmingTools = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Category!.CategoryName == "Farming Tools" && p.IsActive)
                    .Take(4).ToListAsync()
            };

            return View(vm);
        }

        public IActionResult Privacy() => View();

        public IActionResult About() => View();

        public IActionResult Contact() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
