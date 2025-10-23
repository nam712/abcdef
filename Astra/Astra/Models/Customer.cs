
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng khách hàng
    /// </summary>
    [Table("customers")]
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        [MaxLength(50)]
        [Column("customer_code")]
        public string CustomerCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [MaxLength(255)]
        [Column("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

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

        [Required]
        [MaxLength(20)]
        [Column("customer_type")]
        public string CustomerType { get; set; } = "retail";

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        [Column("gender")]
        public string? Gender { get; set; }

        [MaxLength(20)]
        [Column("id_card")]
        public string? IdCard { get; set; }

        [MaxLength(100)]
        [Column("bank_account")]
        public string? BankAccount { get; set; }

        [MaxLength(255)]
        [Column("bank_name")]
        public string? BankName { get; set; }

        [Column("total_debt", TypeName = "decimal(18,2)")]
        public decimal TotalDebt { get; set; } = 0;

        [Column("total_purchase_amount", TypeName = "decimal(18,2)")]
        public decimal TotalPurchaseAmount { get; set; } = 0;

        [Column("total_purchase_count")]
        public int TotalPurchaseCount { get; set; } = 0;

        [Column("loyalty_points")]
        public int LoyaltyPoints { get; set; } = 0;

        [MaxLength(50)]
        [Column("segment")]
        public string? Segment { get; set; }

        [MaxLength(100)]
        [Column("source")]
        public string? Source { get; set; }

        [MaxLength(255)]
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

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
        public virtual ICollection<Invoice>? Invoices { get; set; }
    }
}