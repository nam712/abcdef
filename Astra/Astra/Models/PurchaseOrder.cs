
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng phiếu nhập hàng từ nhà cung cấp
    /// </summary>
    [Table("purchase_orders")]
    public class PurchaseOrder
    {
        [Key]
        [Column("purchase_order_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurchaseOrderId { get; set; }

        [Required(ErrorMessage = "Mã phiếu nhập không được để trống")]
        [MaxLength(50)]
        [Column("po_code")]
        public string PoCode { get; set; } = string.Empty;

        [Required]
        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Required]
        [Column("po_date")]
        public DateTime PoDate { get; set; }

        [Column("expected_delivery_date")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Column("actual_delivery_date")]
        public DateTime? ActualDeliveryDate { get; set; }

        [Column("total_amount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "pending";

        [MaxLength(20)]
        [Column("payment_status")]
        public string PaymentStatus { get; set; } = "unpaid";

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
        public virtual Supplier Supplier { get; set; } = null!;

        public virtual ICollection<PurchaseOrderDetail>? PurchaseOrderDetails { get; set; }
    }
}