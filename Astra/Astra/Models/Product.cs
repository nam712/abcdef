
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng sản phẩm
    /// </summary>
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [MaxLength(50)]
        [Column("product_code")]
        public string ProductCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [MaxLength(255)]
        [Column("product_name")]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [MaxLength(100)]
        [Column("brand")]
        public string? Brand { get; set; }

        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        [Required]
        [Column("price", TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column("cost_price", TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }

        [Required]
        [Column("stock")]
        public int Stock { get; set; } = 0;

        [Column("min_stock")]
        public int MinStock { get; set; } = 0;

        [MaxLength(100)]
        [Column("sku")]
        public string? Sku { get; set; }

        [MaxLength(100)]
        [Column("barcode")]
        public string? Barcode { get; set; }

        [MaxLength(50)]
        [Column("unit")]
        public string? Unit { get; set; }

        [MaxLength(255)]
        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("weight", TypeName = "decimal(10,2)")]
        public decimal? Weight { get; set; }

        [MaxLength(100)]
        [Column("dimension")]
        public string? Dimension { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        [ForeignKey("CategoryId")]
        public virtual ProductCategory? Category { get; set; }

        public virtual ICollection<PurchaseOrderDetail>? PurchaseOrderDetails { get; set; }
        public virtual ICollection<InvoiceDetail>? InvoiceDetails { get; set; }
        public virtual ICollection<PriceHistory>? PriceHistories { get; set; }
    }
}