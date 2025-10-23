
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng quản lý các phương thức thanh toán
    /// </summary>
    [Table("payment_methods")]
    public class PaymentMethod
    {
        [Key]
        [Column("payment_method_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentMethodId { get; set; }

        [Required(ErrorMessage = "Tên phương thức thanh toán không được để trống")]
        [MaxLength(100)]
        [Column("method_name")]
        public string MethodName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã phương thức thanh toán không được để trống")]
        [MaxLength(50)]
        [Column("method_code")]
        public string MethodCode { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Invoice>? Invoices { get; set; }
    }
}