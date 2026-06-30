using System.ComponentModel.DataAnnotations;

namespace ApnaKrishi.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<Cart> CartItems { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal ShippingCharges { get; set; }
        public decimal GrandTotal { get; set; }

        [Required]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Pin Code")]
        public string PinCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        public string? Notes { get; set; }
    }
}
