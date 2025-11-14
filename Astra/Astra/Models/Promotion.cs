using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    [Table("promotions")]
    public class Promotion
    {
        [Key]
        [Column("promotion_id")]
        public int PromotionId { get; set; }

        [Required]
        [Column("promotion_code")]
        [MaxLength(50)]
        public string PromotionCode { get; set; } = string.Empty;

        [Required]
        [Column("promotion_name")]
        [MaxLength(200)]
        public string PromotionName { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("promotion_type")]
        [MaxLength(50)]
        public string PromotionType { get; set; } = string.Empty; // percentage, fixed, buy_x_get_y, free_shipping

        [Required]
        [Column("discount_value")]
        public decimal DiscountValue { get; set; }

        [Column("min_purchase_amount")]
        public decimal? MinPurchaseAmount { get; set; }

        [Column("max_discount_amount")]
        public decimal? MaxDiscountAmount { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "active"; // active, inactive, expired

        [Column("usage_limit")]
        public int? UsageLimit { get; set; }

        [Required]
        [Column("usage_count")]
        public int UsageCount { get; set; } = 0;

        [Column("applicable_products")]
        public string? ApplicableProducts { get; set; } // JSON array of product IDs

        [Column("applicable_customers")]
        public string? ApplicableCustomers { get; set; } // JSON array of customer IDs

        [Required]
        [Column("shop_owner_id")]
        public int ShopOwnerId { get; set; }

        [Column("invoice_id")]
        public int? InvoiceId { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("InvoiceId")]
        public virtual Invoice? Invoice { get; set; }
    }
}
