
using Backend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourShopManagement.API.Models
{
    /// <summary>
    /// Bảng hóa đơn
    /// </summary>
    [Table("invoices")]
    public class Invoice
    {
        [Key]
        [Column("invoice_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Mã hóa đơn không được để trống")]
        [MaxLength(50)]
        [Column("invoice_code")]
        public string InvoiceCode { get; set; } = string.Empty;

        [Required]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Required]
        [Column("invoice_date")]
        public DateTime InvoiceDate { get; set; }

        [Required]
        [Column("total_amount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column("discount_amount", TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [Column("final_amount", TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        [Column("amount_paid", TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; } = 0;

        [Column("payment_method_id")]
        public int? PaymentMethodId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("payment_status")]
        public string PaymentStatus { get; set; } = "unpaid";

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod PaymentMethod { get; set; }

        public virtual ICollection<InvoiceDetail>? InvoiceDetails { get; set; }
    }
}