using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaKrishi.Models
{
    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed,
        Refunded
    }

    public class Payment
    {
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [StringLength(200)]
        public string? TransactionId { get; set; }

        [StringLength(500)]
        public string? RazorpayOrderId { get; set; }

        [StringLength(500)]
        public string? RazorpayPaymentId { get; set; }

        // Navigation
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}
