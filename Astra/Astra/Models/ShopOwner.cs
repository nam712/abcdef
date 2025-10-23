using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng chủ shop
    /// </summary>
    [Table("shop_owner")]
    public class ShopOwner
    {
        [Key]
        [Column("shop_owner_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShopOwnerId { get; set; }

        [Required(ErrorMessage = "Tên chủ shop không được để trống")]
        [MaxLength(255)]
        [Column("shop_owner_name")]
        public string ShopOwnerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [MaxLength(20)]
        [Column("phone")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(255)]
        [Column("address")]
        public string? Address { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        [Column("gender")]
        public string? Gender { get; set; }

        [MaxLength(20)]
        [Column("id_card_number")]
        public string? IdCardNumber { get; set; }

        [MaxLength(255)]
        [Column("id_card_issued_place")]
        public string? IdCardIssuedPlace { get; set; }

        [Column("id_card_issued_date")]
        public DateTime? IdCardIssuedDate { get; set; }

        [MaxLength(50)]
        [Column("tax_code")]
        public string? TaxCode { get; set; }

        [MaxLength(50)]
        [Column("business_license_number")]
        public string? BusinessLicenseNumber { get; set; }

        [Column("business_license_issued_date")]
        public DateTime? BusinessLicenseIssuedDate { get; set; }

        [MaxLength(255)]
        [Column("business_license_issued_place")]
        public string? BusinessLicenseIssuedPlace { get; set; }

        [Column("business_category_id")]
        public int? BusinessCategoryId { get; set; }

        [Required(ErrorMessage = "Tên cửa hàng không được để trống")]
        [MaxLength(255)]
        [Column("shop_name")]
        public string ShopName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("shop_logo_url")]
        public string? ShopLogoUrl { get; set; }

        [MaxLength(255)]
        [Column("shop_description")]
        public string? ShopDescription { get; set; }

        [MaxLength(255)]
        [Column("shop_address")]
        public string? ShopAddress { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MaxLength(255)]
        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Required]
        [Column("terms_and_conditions_agreed")]
        public bool TermsAndConditionsAgreed { get; set; } = false;

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
        [ForeignKey("BusinessCategoryId")]
        public virtual BusinessCategory? BusinessCategory { get; set; }
    }
}