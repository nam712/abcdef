using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng chi nhánh/cửa hàng (shops)
    /// </summary>
    [Table("shops")]
    public class Shop
    {
        [Key]
        [Column("shop_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShopId { get; set; }

        [Required]
        [Column("shop_owner_id")]
        public int ShopOwnerId { get; set; }

        [Required(ErrorMessage = "Mã cửa hàng không được để trống")]
        [MaxLength(50)]
        [Column("shop_code")]
        public string ShopCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên cửa hàng không được để trống")]
        [MaxLength(255)]
        [Column("shop_name")]
        public string ShopName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("shop_address")]
        public string? ShopAddress { get; set; }

        [MaxLength(20)]
        [Column("shop_phone")]
        public string? ShopPhone { get; set; }

        [MaxLength(100)]
        [Column("shop_email")]
        public string? ShopEmail { get; set; }

        [MaxLength(255)]
        [Column("manager_name")]
        public string? ManagerName { get; set; }

        [MaxLength(20)]
        [Column("manager_phone")]
        public string? ManagerPhone { get; set; }

        [Column("business_category_id")]
        public int? BusinessCategoryId { get; set; }

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
        [ForeignKey("ShopOwnerId")]
        public virtual ShopOwner ShopOwner { get; set; } = null!;

        [ForeignKey("BusinessCategoryId")]
        public virtual BusinessCategory? BusinessCategory { get; set; }

        public virtual ICollection<Invoice>? Invoices { get; set; }
    }
}
