using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng lịch sử giá bán sản phẩm
    /// </summary>
    [Table("price_history")]
    public class PriceHistory
    {
        [Key]
        [Column("price_history_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PriceHistoryId { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("old_price", TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        [Required]
        [Column("new_price", TypeName = "decimal(18,2)")]
        public decimal NewPrice { get; set; }

        [MaxLength(255)]
        [Column("reason")]
        public string? Reason { get; set; }

        [Required]
        [Column("effective_date")]
        public DateTime EffectiveDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}