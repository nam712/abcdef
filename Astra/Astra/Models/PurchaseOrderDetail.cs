using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng chi tiết phiếu nhập hàng
    /// </summary>
    [Table("purchase_order_details")]
    public class PurchaseOrderDetail
    {
        [Key]
        [Column("purchase_order_detail_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurchaseOrderDetailId { get; set; }

        [Required]
        [Column("purchase_order_id")]
        public int PurchaseOrderId { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("import_price", TypeName = "decimal(18,2)")]
        public decimal ImportPrice { get; set; }

        [Required]
        [Column("final_amount", TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        // Navigation Properties
        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}