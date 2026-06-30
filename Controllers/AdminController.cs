using ApnaKrishi.Data;
using ApnaKrishi.Models;
using ApnaKrishi.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApnaKrishi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // ══════════════════════════════════════════════════════════════════════
        // DASHBOARD
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            var allUsers   = await _userManager.GetUsersInRoleAsync("Customer");
            var orders     = await _context.Orders.ToListAsync();
            var today      = DateTime.Today;

            var monthlyData = await _context.Orders
                .Where(o => o.OrderDate.Year == DateTime.Now.Year)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new MonthlySales
                {
                    Month      = g.Key.ToString(),
                    Amount     = g.Sum(o => o.GrandTotal),
                    OrderCount = g.Count()
                }).ToListAsync();

            // Top 5 selling products
            var topProducts = await _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => new { od.ProductId, od.Product!.ProductName })
                .Select(g => new TopProduct
                {
                    ProductName  = g.Key.ProductName,
                    TotalQty     = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.TotalQty)
                .Take(5)
                .ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalUsers        = allUsers.Count,
                TotalProducts     = await _context.Products.CountAsync(),
                TotalOrders       = orders.Count,
                TotalSales        = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.GrandTotal),
                PendingOrders     = orders.Count(o => o.Status == OrderStatus.Pending),
                AcceptedOrders    = orders.Count(o => o.Status == OrderStatus.Accepted),
                DispatchedOrders  = orders.Count(o => o.Status == OrderStatus.Dispatched),
                DeliveredOrders   = orders.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders   = orders.Count(o => o.Status == OrderStatus.Cancelled),
                TodayOrders       = orders.Count(o => o.OrderDate.Date == today),
                TodaySales        = orders.Where(o => o.OrderDate.Date == today).Sum(o => o.GrandTotal),
                RecentOrders      = await _context.Orders
                                        .Include(o => o.User)
                                        .OrderByDescending(o => o.OrderDate)
                                        .Take(10).ToListAsync(),
                MonthlySalesData  = monthlyData,
                TopSellingProducts = topProducts
            };

            return View(vm);
        }

        // ══════════════════════════════════════════════════════════════════════
        // CATEGORY MANAGEMENT
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Categories() =>
            View(await _context.Categories.OrderByDescending(c => c.CreatedDate).ToListAsync());

        [HttpGet]
        public IActionResult AddCategory() => View(new Category());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(Category model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);

            if (imageFile != null)
                model.ImageUrl = await SaveImageAsync(imageFile, "categories");

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Category '{model.CategoryName}' added successfully.";
            return RedirectToAction("Categories");
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Category model, IFormFile? imageFile)
        {
            var cat = await _context.Categories.FindAsync(model.CategoryId);
            if (cat == null) return NotFound();

            cat.CategoryName = model.CategoryName;
            cat.Description  = model.Description;
            cat.IsActive     = model.IsActive;

            if (imageFile != null)
                cat.ImageUrl = await SaveImageAsync(imageFile, "categories");

            await _context.SaveChangesAsync();
            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (cat == null) return NotFound();

            if (cat.Products.Any())
            {
                TempData["Error"] = $"Cannot delete '{cat.CategoryName}' — it has {cat.Products.Count} products. Remove or reassign them first.";
                return RedirectToAction("Categories");
            }

            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Categories");
        }

        // ══════════════════════════════════════════════════════════════════════
        // PRODUCT MANAGEMENT
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Products(string? search, int? categoryId, string? stockFilter)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.ProductName.Contains(search) ||
                                         p.Description!.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (stockFilter == "low")
                query = query.Where(p => p.Stock > 0 && p.Stock <= 10);
            else if (stockFilter == "out")
                query = query.Where(p => p.Stock == 0);

            ViewBag.Categories  = await _context.Categories.ToListAsync();
            ViewBag.Search      = search;
            ViewBag.CategoryId  = categoryId;
            ViewBag.StockFilter = stockFilter;

            return View(await query.OrderByDescending(p => p.CreatedDate).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct() =>
            View(new ProductFormViewModel
            {
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync()
            });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductFormViewModel model)
        {
            model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            if (!ModelState.IsValid) return View(model);

            var product = new Product
            {
                CategoryId      = model.CategoryId,
                ProductName     = model.ProductName,
                Description     = model.Description,
                Price           = model.Price,
                DiscountPercent = model.DiscountPercent,
                Stock           = model.Stock,
                IsFeatured      = model.IsFeatured,
                IsBestSeller    = model.IsBestSeller,
                IsNewArrival    = model.IsNewArrival,
                IsActive        = model.IsActive
            };

            if (model.ImageFile != null)
                product.ImageUrl = await SaveImageAsync(model.ImageFile, "products");

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Product '{product.ProductName}' added successfully.";
            return RedirectToAction("Products");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();

            return View(new ProductFormViewModel
            {
                ProductId        = p.ProductId,
                CategoryId       = p.CategoryId,
                ProductName      = p.ProductName,
                Description      = p.Description,
                Price            = p.Price,
                DiscountPercent  = p.DiscountPercent,
                Stock            = p.Stock,
                ExistingImageUrl = p.ImageUrl,
                IsFeatured       = p.IsFeatured,
                IsBestSeller     = p.IsBestSeller,
                IsNewArrival     = p.IsNewArrival,
                IsActive         = p.IsActive,
                Categories       = await _context.Categories.Where(c => c.IsActive).ToListAsync()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductFormViewModel model)
        {
            model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            if (!ModelState.IsValid) return View(model);

            var p = await _context.Products.FindAsync(model.ProductId);
            if (p == null) return NotFound();

            p.CategoryId      = model.CategoryId;
            p.ProductName     = model.ProductName;
            p.Description     = model.Description;
            p.Price           = model.Price;
            p.DiscountPercent = model.DiscountPercent;
            p.Stock           = model.Stock;
            p.IsFeatured      = model.IsFeatured;
            p.IsBestSeller    = model.IsBestSeller;
            p.IsNewArrival    = model.IsNewArrival;
            p.IsActive        = model.IsActive;

            if (model.ImageFile != null)
                p.ImageUrl = await SaveImageAsync(model.ImageFile, "products");

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();
            _context.Products.Remove(p);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product deleted.";
            return RedirectToAction("Products");
        }

        // Quick stock update (inline from product list)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int productId, int stock)
        {
            var p = await _context.Products.FindAsync(productId);
            if (p != null)
            {
                p.Stock = stock;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Stock updated for '{p.ProductName}'.";
            }
            return RedirectToAction("Products");
        }

        // ══════════════════════════════════════════════════════════════════════
        // USER MANAGEMENT
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Users(string? search, string? filter)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                users = users.Where(u => u.FullName.Contains(search) ||
                                         u.Email!.Contains(search) ||
                                         (u.Mobile != null && u.Mobile.Contains(search)));

            if (filter == "blocked")
                users = users.Where(u => u.IsBlocked);

            var list = await users.OrderByDescending(u => u.CreatedDate).ToListAsync();

            // Exclude admins from the list
            var admins = (await _userManager.GetUsersInRoleAsync("Admin")).Select(a => a.Id).ToHashSet();
            list = list.Where(u => !admins.Contains(u.Id)).ToList();

            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.TotalUsers   = list.Count;
            ViewBag.BlockedUsers = list.Count(u => u.IsBlocked);
            return View(list);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.Orders     = orders;
            ViewBag.TotalSpent = orders.Sum(o => o.GrandTotal);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBlock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsBlocked = !user.IsBlocked;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = user.IsBlocked
                    ? $"{user.FullName} has been blocked."
                    : $"{user.FullName} has been unblocked.";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["Success"] = "User deleted permanently.";
            }
            return RedirectToAction("Users");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ORDER MANAGEMENT
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Orders(string? status, string? search,
            DateTime? from, DateTime? to)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<OrderStatus>(status, out var s))
                query = query.Where(o => o.Status == s);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o =>
                    o.User!.FullName.Contains(search) ||
                    o.User.Email!.Contains(search) ||
                    o.OrderId.ToString().Contains(search));

            if (from.HasValue)
                query = query.Where(o => o.OrderDate.Date >= from.Value.Date);
            if (to.HasValue)
                query = query.Where(o => o.OrderDate.Date <= to.Value.Date);

            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.From   = from?.ToString("yyyy-MM-dd");
            ViewBag.To     = to?.ToString("yyyy-MM-dd");

            // Order counts per status for badge display
            var allOrders = await _context.Orders.ToListAsync();
            ViewBag.Counts = new Dictionary<string, int>
            {
                ["All"]        = allOrders.Count,
                ["Pending"]    = allOrders.Count(o => o.Status == OrderStatus.Pending),
                ["Accepted"]   = allOrders.Count(o => o.Status == OrderStatus.Accepted),
                ["Dispatched"] = allOrders.Count(o => o.Status == OrderStatus.Dispatched),
                ["Delivered"]  = allOrders.Count(o => o.Status == OrderStatus.Delivered),
                ["Cancelled"]  = allOrders.Count(o => o.Status == OrderStatus.Cancelled),
                ["Rejected"]   = allOrders.Count(o => o.Status == OrderStatus.Rejected),
            };

            return View(await query.OrderByDescending(o => o.OrderDate).ToListAsync());
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p!.Category)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Order #{orderId} status updated to <strong>{status}</strong>.";
            return RedirectToAction("OrderDetails", new { id = orderId });
        }

        // Quick status change from order list (Accept / Reject / Dispatch / Complete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickStatusChange(int orderId, OrderStatus status,
            string? returnStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Order #{orderId} → {status}.";
            }
            return RedirectToAction("Orders", new { status = returnStatus });
        }

        // ══════════════════════════════════════════════════════════════════════
        // PAYMENT MANAGEMENT
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Payments(string? status, string? search)
        {
            var query = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o!.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<PaymentStatus>(status, out var ps))
                query = query.Where(p => p.PaymentStatus == ps);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.TransactionId!.Contains(search) ||
                    p.Order!.User!.FullName.Contains(search) ||
                    p.OrderId.ToString().Contains(search));

            ViewBag.Status = status;
            ViewBag.Search = search;

            var allPayments = await _context.Payments.ToListAsync();
            ViewBag.PaymentCounts = new Dictionary<string, int>
            {
                ["All"]      = allPayments.Count,
                ["Pending"]  = allPayments.Count(p => p.PaymentStatus == PaymentStatus.Pending),
                ["Success"]  = allPayments.Count(p => p.PaymentStatus == PaymentStatus.Success),
                ["Failed"]   = allPayments.Count(p => p.PaymentStatus == PaymentStatus.Failed),
                ["Refunded"] = allPayments.Count(p => p.PaymentStatus == PaymentStatus.Refunded),
            };
            ViewBag.TotalCollected = allPayments
                .Where(p => p.PaymentStatus == PaymentStatus.Success)
                .Sum(p => p.Amount);

            return View(await query.OrderByDescending(p => p.PaymentDate).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefundPayment(int paymentId, string? reason)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null) return NotFound();

            payment.PaymentStatus = PaymentStatus.Refunded;

            // Also update the order status to Cancelled
            if (payment.Order != null)
                payment.Order.Status = OrderStatus.Cancelled;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Payment #{paymentId} marked as refunded and order cancelled.";
            return RedirectToAction("Payments");
        }

        // ══════════════════════════════════════════════════════════════════════
        // REPORTS
        // ══════════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Reports(string type = "daily",
            DateTime? from = null, DateTime? to = null)
        {
            from ??= DateTime.Today;
            to   ??= DateTime.Today;

            var vm = new ReportsViewModel
            {
                ReportType = type,
                FromDate   = from,
                ToDate     = to
            };

            switch (type)
            {
                case "daily":
                    var dailyOrders = await _context.Orders
                        .Include(o => o.User)
                        .Include(o => o.Payment)
                        .Where(o => o.OrderDate.Date >= from.Value.Date &&
                                    o.OrderDate.Date <= to.Value.Date)
                        .OrderByDescending(o => o.OrderDate)
                        .ToListAsync();

                    vm.DailyOrders     = dailyOrders;
                    vm.DailyOrderCount = dailyOrders.Count;
                    vm.DailyRevenue    = dailyOrders.Sum(o => o.GrandTotal);
                    break;

                case "monthly":
                    var monthlyRaw = await _context.Orders
                        .Where(o => o.Status == OrderStatus.Delivered)
                        .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                        .Select(g => new MonthlySalesRow
                        {
                            Year       = g.Key.Year,
                            Month      = g.Key.Month,
                            OrderCount = g.Count(),
                            Revenue    = g.Sum(o => o.GrandTotal),
                            ItemsSold  = 0 // computed below
                        })
                        .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
                        .ToListAsync();

                    vm.MonthlyData  = monthlyRaw;
                    vm.MonthlyTotal = monthlyRaw.Sum(r => r.Revenue);
                    break;

                case "product":
                    vm.ProductSalesData = await _context.OrderDetails
                        .Include(od => od.Product)
                            .ThenInclude(p => p!.Category)
                        .Include(od => od.Order)
                        .Where(od => od.Order!.OrderDate.Date >= from.Value.Date &&
                                     od.Order.OrderDate.Date <= to.Value.Date)
                        .GroupBy(od => new
                        {
                            od.ProductId,
                            Name     = od.Product!.ProductName,
                            Category = od.Product.Category!.CategoryName
                        })
                        .Select(g => new ProductSalesRow
                        {
                            ProductId          = g.Key.ProductId,
                            ProductName        = g.Key.Name,
                            CategoryName       = g.Key.Category,
                            TotalQuantitySold  = g.Sum(x => x.Quantity),
                            TotalRevenue       = g.Sum(x => x.Quantity * x.Price),
                            OrderCount         = g.Select(x => x.OrderId).Distinct().Count()
                        })
                        .OrderByDescending(r => r.TotalRevenue)
                        .ToListAsync();
                    break;

                case "customer":
                    var customerOrders = await _context.Orders
                        .Include(o => o.User)
                        .Where(o => o.OrderDate.Date >= from.Value.Date &&
                                    o.OrderDate.Date <= to.Value.Date)
                        .ToListAsync();

                    vm.CustomerData = customerOrders
                        .GroupBy(o => new
                        {
                            o.UserId,
                            o.User!.FullName,
                            Email    = o.User.Email ?? "",
                            Mobile   = o.User.Mobile ?? "",
                            Joined   = o.User.CreatedDate
                        })
                        .Select(g => new CustomerReportRow
                        {
                            CustomerId      = g.Key.UserId,
                            CustomerName    = g.Key.FullName,
                            Email           = g.Key.Email,
                            Mobile          = g.Key.Mobile,
                            TotalOrders     = g.Count(),
                            TotalSpent      = g.Sum(o => o.GrandTotal),
                            LastOrderDate   = g.Max(o => o.OrderDate),
                            RegisteredDate  = g.Key.Joined
                        })
                        .OrderByDescending(r => r.TotalSpent)
                        .ToList();
                    break;
            }

            return View(vm);
        }

        // ══════════════════════════════════════════════════════════════════════
        // SETTINGS
        // ══════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var settings = await _context.WebsiteSettings.FindAsync(1)
                           ?? new WebsiteSettings();

            var vm = new WebsiteSettingsViewModel
            {
                SiteName               = settings.SiteName,
                Tagline                = settings.Tagline,
                ContactEmail           = settings.ContactEmail,
                ContactPhone           = settings.ContactPhone,
                Address                = settings.Address,
                FreeShippingThreshold  = settings.FreeShippingThreshold,
                ShippingCharge         = settings.ShippingCharge,
                GstPercent             = settings.GstPercent,
                MaintenanceMode        = settings.MaintenanceMode,
                AllowRegistrations     = settings.AllowRegistrations,
                MetaDescription        = settings.MetaDescription,
                FacebookUrl            = settings.FacebookUrl,
                InstagramUrl           = settings.InstagramUrl,
                TwitterUrl             = settings.TwitterUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(WebsiteSettingsViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var settings = await _context.WebsiteSettings.FindAsync(1);
            if (settings == null)
            {
                settings = new WebsiteSettings { Id = 1 };
                _context.WebsiteSettings.Add(settings);
            }

            settings.SiteName              = model.SiteName;
            settings.Tagline               = model.Tagline;
            settings.ContactEmail          = model.ContactEmail;
            settings.ContactPhone          = model.ContactPhone;
            settings.Address               = model.Address;
            settings.FreeShippingThreshold = model.FreeShippingThreshold;
            settings.ShippingCharge        = model.ShippingCharge;
            settings.GstPercent            = model.GstPercent;
            settings.MaintenanceMode       = model.MaintenanceMode;
            settings.AllowRegistrations    = model.AllowRegistrations;
            settings.MetaDescription       = model.MetaDescription;
            settings.FacebookUrl           = model.FacebookUrl;
            settings.InstagramUrl          = model.InstagramUrl;
            settings.TwitterUrl            = model.TwitterUrl;
            settings.UpdatedAt             = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Website settings saved successfully.";
            return RedirectToAction("Settings");
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPER
        // ══════════════════════════════════════════════════════════════════════
        private async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{folder}/{fileName}";
        }
    }
}
