using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("momoinfos")]
    public class MomoInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("order_id")]
        [MaxLength(255)]
        public string OrderId { get; set; }

        [Required]
        [Column("order_info", TypeName = "text")]
        public string OrderInfo { get; set; }

        [Column("full_name")]
        [MaxLength(255)]
        public string Fullname { get; set; }

        [Column("amount", TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        [Column("date_paid")]
        public DateTime? DatePaid { get; set; }

        [Required]
        [Column("payment_method_id")]
        public int PaymentMethodId { get; set; }

        // Navigation property
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod PaymentMethod { get; set; }
    }
}
