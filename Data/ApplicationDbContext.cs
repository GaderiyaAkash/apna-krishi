using ApnaKrishi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApnaKrishi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<WebsiteSettings> WebsiteSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed WebsiteSettings (singleton row)
            builder.Entity<WebsiteSettings>().HasData(
                new WebsiteSettings { Id = 1 }
            );

            // Seed Categories
            builder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Fertilizers", Description = "Organic and chemical fertilizers for better yield", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Category { CategoryId = 2, CategoryName = "Seeds", Description = "High quality seeds for all crops", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Category { CategoryId = 3, CategoryName = "Pesticides", Description = "Effective pesticides to protect your crops", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Category { CategoryId = 4, CategoryName = "Farming Tools", Description = "Modern and traditional farming equipment", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) }
            );

            // Seed Products
            builder.Entity<Product>().HasData(
                new Product { ProductId = 1, CategoryId = 1, ProductName = "DAP Fertilizer 50kg", Description = "Di-ammonium phosphate fertilizer, best for wheat and rice", Price = 1350, DiscountPercent = 5, Stock = 200, IsFeatured = true, IsBestSeller = true, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Product { ProductId = 2, CategoryId = 1, ProductName = "Urea 50kg Bag", Description = "High nitrogen content urea for fast growth", Price = 266, DiscountPercent = 0, Stock = 500, IsFeatured = true, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Product { ProductId = 3, CategoryId = 2, ProductName = "Wheat Seeds HD-2967", Description = "High yielding wheat variety, rust resistant", Price = 450, DiscountPercent = 10, Stock = 300, IsFeatured = true, IsNewArrival = true, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Product { ProductId = 4, CategoryId = 2, ProductName = "Hybrid Tomato Seeds", Description = "F1 hybrid tomato seeds, disease resistant", Price = 280, DiscountPercent = 0, Stock = 150, IsBestSeller = true, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Product { ProductId = 5, CategoryId = 3, ProductName = "Chlorpyrifos 20% EC 1L", Description = "Broad spectrum insecticide for cotton and vegetables", Price = 320, DiscountPercent = 8, Stock = 100, IsFeatured = true, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Product { ProductId = 6, CategoryId = 3, ProductName = "Mancozeb 75% WP 500g", Description = "Fungicide for controlling blight and rust diseases", Price = 180, DiscountPercent = 0, Stock = 80, IsNewArrival = true, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Product { ProductId = 7, CategoryId = 4, ProductName = "Hand Sprayer Pump 16L", Description = "Battery operated backpack sprayer with 16L tank", Price = 1850, DiscountPercent = 12, Stock = 50, IsFeatured = true, IsBestSeller = true, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Product { ProductId = 8, CategoryId = 4, ProductName = "Garden Hoe (Khurpi Set)", Description = "Stainless steel khurpi set for weeding", Price = 350, DiscountPercent = 0, Stock = 120, IsNewArrival = true, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) }
            );
        }
    }
}
