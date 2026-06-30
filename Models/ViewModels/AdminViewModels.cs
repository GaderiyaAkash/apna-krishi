using System.ComponentModel.DataAnnotations;

namespace ApnaKrishi.Models.ViewModels
{
    // ── Dashboard ──────────────────────────────────────────────────────────────
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public int PendingOrders { get; set; }
        public int AcceptedOrders { get; set; }
        public int DispatchedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int TodayOrders { get; set; }
        public decimal TodaySales { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<MonthlySales> MonthlySalesData { get; set; } = new();
        public List<TopProduct> TopSellingProducts { get; set; } = new();
    }

    public class MonthlySales
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProduct
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQty { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // ── Product Form ──────────────────────────────────────────────────────────
    public class ProductFormViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be 0–100")]
        [Display(Name = "Discount (%)")]
        public decimal DiscountPercent { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsActive { get; set; } = true;

        public List<Category> Categories { get; set; } = new();
    }

    // ── Reports ───────────────────────────────────────────────────────────────
    public class ReportsViewModel
    {
        public string ReportType { get; set; } = "daily";

        // Daily
        public List<Order> DailyOrders { get; set; } = new();
        public decimal DailyRevenue { get; set; }
        public int DailyOrderCount { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.Today;

        // Monthly
        public List<MonthlySalesRow> MonthlyData { get; set; } = new();
        public decimal MonthlyTotal { get; set; }

        // Product Sales
        public List<ProductSalesRow> ProductSalesData { get; set; } = new();

        // Customer Report
        public List<CustomerReportRow> CustomerData { get; set; } = new();

        // Filter
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class MonthlySalesRow
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public int ItemsSold { get; set; }
    }

    public class ProductSalesRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class CustomerReportRow
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
        public DateTime RegisteredDate { get; set; }
    }

    // ── Website Settings ──────────────────────────────────────────────────────
    public class WebsiteSettingsViewModel
    {
        [Required]
        [Display(Name = "Site Name")]
        public string SiteName { get; set; } = "Apna Krishi";

        [Display(Name = "Tagline")]
        public string Tagline { get; set; } = "Online Agriculture Product Store";

        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = "support@apnakrishi.com";

        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = "+91 9999999999";

        [Display(Name = "Address")]
        public string Address { get; set; } = "India";

        [Display(Name = "Free Shipping Threshold (₹)")]
        [Range(0, 100000)]
        public decimal FreeShippingThreshold { get; set; } = 500;

        [Display(Name = "Flat Shipping Charge (₹)")]
        [Range(0, 5000)]
        public decimal ShippingCharge { get; set; } = 60;

        [Display(Name = "GST Percentage (%)")]
        [Range(0, 28)]
        public decimal GstPercent { get; set; } = 0;

        [Display(Name = "Maintenance Mode")]
        public bool MaintenanceMode { get; set; } = false;

        [Display(Name = "Allow New Registrations")]
        public bool AllowRegistrations { get; set; } = true;

        [Display(Name = "Meta Description")]
        [StringLength(300)]
        public string MetaDescription { get; set; } = "Buy quality agricultural products online – seeds, fertilizers, pesticides and farming tools.";

        [Display(Name = "Facebook URL")]
        public string? FacebookUrl { get; set; }

        [Display(Name = "Instagram URL")]
        public string? InstagramUrl { get; set; }

        [Display(Name = "Twitter/X URL")]
        public string? TwitterUrl { get; set; }
    }
}
