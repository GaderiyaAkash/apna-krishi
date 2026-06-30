using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaKrishi.Models
{
    /// <summary>
    /// Singleton row (Id = 1) — stores site-wide configuration.
    /// </summary>
    public class WebsiteSettings
    {
        [Key]
        public int Id { get; set; } = 1;

        [StringLength(200)]
        public string SiteName { get; set; } = "Apna Krishi";

        [StringLength(500)]
        public string Tagline { get; set; } = "Online Agriculture Product Store";

        [StringLength(200)]
        public string ContactEmail { get; set; } = "support@apnakrishi.com";

        [StringLength(50)]
        public string ContactPhone { get; set; } = "+91 9999999999";

        [StringLength(500)]
        public string Address { get; set; } = "India";

        [Column(TypeName = "decimal(10,2)")]
        public decimal FreeShippingThreshold { get; set; } = 500;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingCharge { get; set; } = 60;

        [Column(TypeName = "decimal(5,2)")]
        public decimal GstPercent { get; set; } = 0;

        public bool MaintenanceMode { get; set; } = false;

        public bool AllowRegistrations { get; set; } = true;

        [StringLength(300)]
        public string MetaDescription { get; set; } = "Buy quality agricultural products online.";

        [StringLength(300)]
        public string? FacebookUrl { get; set; }

        [StringLength(300)]
        public string? InstagramUrl { get; set; }

        [StringLength(300)]
        public string? TwitterUrl { get; set; }

        public DateTime UpdatedAt { get; set; } = new DateTime(2024, 1, 1);
    }
}
