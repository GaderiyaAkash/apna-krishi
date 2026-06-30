using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaKrishi.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Navigation
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [NotMapped]
        public decimal SubTotal => Price * Quantity;
    }
}
