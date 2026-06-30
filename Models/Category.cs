using System.ComponentModel.DataAnnotations;

namespace ApnaKrishi.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
