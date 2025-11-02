using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng loại sản phẩm (danh mục sản phẩm)
    /// </summary>
    [Table("product_categories")]
    public class ProductCategory
    {
        [Key]
        [Column("category_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        // ✅ THÊM PROPERTY LIÊN KẾT ĐẾN SHOP OWNER
        [Required]
        [Column("shop_owner_id")]
        public int ShopOwnerId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MaxLength(255)]
        [Column("category_name")]
        public string CategoryName { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("parent_category_id")]
        public int? ParentCategoryId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "active";

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ✅ NAVIGATION PROPERTY LIÊN KẾT VỚI SHOP OWNER
        [ForeignKey("ShopOwnerId")]
        public virtual ShopOwner ShopOwner { get; set; } = null!;

        // Navigation Properties
        [ForeignKey("ParentCategoryId")]
        public virtual ProductCategory? ParentCategory { get; set; }
        public virtual ICollection<ProductCategory>? Children { get; set; }
        public virtual ICollection<Product>? Products { get; set; }
    }
}