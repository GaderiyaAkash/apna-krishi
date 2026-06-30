using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaKrishi.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercent { get; set; } = 0;

        public int Stock { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public bool IsFeatured { get; set; } = false;

        public bool IsBestSeller { get; set; } = false;

        public bool IsNewArrival { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        [NotMapped]
        public decimal DiscountedPrice => Price - (Price * DiscountPercent / 100);

        [NotMapped]
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
    }
}
