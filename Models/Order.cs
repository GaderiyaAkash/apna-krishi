using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaKrishi.Models
{
    public enum OrderStatus
    {
        Pending,
        Accepted,
        Dispatched,
        Delivered,
        Cancelled,
        Rejected
    }

    public enum PaymentMethod
    {
        CashOnDelivery,
        UPI,
        CreditCard,
        DebitCard,
        NetBanking,
        Razorpay
    }

    public class Order
    {
        public int OrderId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingCharges { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal GrandTotal { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? PinCode { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public Payment? Payment { get; set; }
    }
}
