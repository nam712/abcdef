using System;
using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs
{

    public class CustomerDto
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        [MaxLength(50)]
        public string CustomerCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [MaxLength(255)]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public string? CustomerType { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? IdCard { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public decimal? TotalDebt { get; set; }
        public decimal? TotalPurchaseAmount { get; set; }
        public int? TotalPurchaseCount { get; set; }
        public int? LoyaltyPoints { get; set; }
        public string? Segment { get; set; }
        public string? Source { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
