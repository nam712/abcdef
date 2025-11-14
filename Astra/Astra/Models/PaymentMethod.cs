// PaymentMethod entity class for EF Core mapping to payment_methods table
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YourShopManagement.API.Models;

namespace Backend.Models
{
    [Table("payment_methods")]
    public class PaymentMethod
    {
        [Key]
        [Column("payment_method_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentMethodId { get; set; }

        [Required]
        [Column("method_name")]
        [MaxLength(100)]
        public string MethodName { get; set; } = string.Empty;

        [Required]
        [Column("method_code")]
        [MaxLength(50)]
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

        // Navigation property tới hóa đơn
        public virtual ICollection<Invoice>? Invoices { get; set; }
        
        // Navigation property tới MomoInfo
        public virtual ICollection<MomoInfo>? MomoInfos { get; set; }
    }
}