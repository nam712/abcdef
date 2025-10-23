
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng ngành hàng
    /// </summary>
    [Table("business_categories")]
    public class BusinessCategory
    {
        [Key]
        [Column("category_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên ngành hàng không được để trống")]
        [MaxLength(255)]
        [Column("category_name")]
        public string CategoryName { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ICollection<ShopOwner>? ShopOwners { get; set; }
    }
}