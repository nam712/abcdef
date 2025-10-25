// PaymentMethod entity class for EF Core mapping to payment_methods table
using System;
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
        public int PaymentMethodId { get; set; }

        [Required]
        [Column("method_name")]
        [MaxLength(100)]
        public string MethodName { get; set; }

        [Required]
        [Column("method_code")]
        [MaxLength(50)]
        public string MethodCode { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Invoice>? Invoices { get; set; }

    }
}
