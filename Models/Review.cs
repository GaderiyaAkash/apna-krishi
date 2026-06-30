using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaKrishi.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
