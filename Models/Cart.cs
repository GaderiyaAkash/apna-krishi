using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaKrishi.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [NotMapped]
        public decimal TotalPrice => (Product?.DiscountedPrice ?? 0) * Quantity;
    }
}
