
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng nhà cung cấp
    /// </summary>
    [Table("suppliers")]
    public class Supplier
    {
        [Key]
        [Column("supplier_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Mã nhà cung cấp không được để trống")]
        [MaxLength(50)]
        [Column("supplier_code")]
        public string SupplierCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        [MaxLength(255)]
        [Column("supplier_name")]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("contact_name")]
        public string? ContactName { get; set; }

        [MaxLength(20)]
        [Column("phone")]
        public string? Phone { get; set; }

        [MaxLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(255)]
        [Column("address")]
        public string? Address { get; set; }

        [MaxLength(50)]
        [Column("tax_code")]
        public string? TaxCode { get; set; }

        [MaxLength(100)]
        [Column("bank_account")]
        public string? BankAccount { get; set; }

        [MaxLength(255)]
        [Column("bank_name")]
        public string? BankName { get; set; }

        [MaxLength(255)]
        [Column("price_list")]
        public string? PriceList { get; set; }

        [MaxLength(255)]
        [Column("logo_url")]
        public string? LogoUrl { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Product>? Products { get; set; }
        public virtual ICollection<PurchaseOrder>? PurchaseOrders { get; set; }
    }
}