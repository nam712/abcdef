using System;
using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.Supplier
{
    // ==================== SUPPLIER INFO DTO ====================
    public class SupplierInfoDto
    {
        public int SupplierId { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? PriceList { get; set; }
        public string? LogoUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // ==================== CREATE SUPPLIER DTO ====================
    public class CreateSupplierDto
    {
        [Required(ErrorMessage = "Mã nhà cung cấp không được để trống")]
        [MaxLength(50, ErrorMessage = "Mã nhà cung cấp không được quá 50 ký tự")]
        public string SupplierCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        [MaxLength(255, ErrorMessage = "Tên nhà cung cấp không được quá 255 ký tự")]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Tên người liên hệ không được quá 255 ký tự")]
        public string? ContactPerson { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        public string? Email { get; set; }

        [MaxLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        public string? Address { get; set; }

        [MaxLength(50, ErrorMessage = "Mã số thuế không được quá 50 ký tự")]
        public string? TaxCode { get; set; }

        [MaxLength(100, ErrorMessage = "Số tài khoản ngân hàng không được quá 100 ký tự")]
        public string? BankAccount { get; set; }

        [MaxLength(255, ErrorMessage = "Tên ngân hàng không được quá 255 ký tự")]
        public string? BankName { get; set; }

        [MaxLength(255, ErrorMessage = "Bảng giá không được quá 255 ký tự")]
        public string? PriceList { get; set; }

        [MaxLength(255, ErrorMessage = "URL logo không được quá 255 ký tự")]
        public string? LogoUrl { get; set; }

        [MaxLength(20, ErrorMessage = "Trạng thái không được quá 20 ký tự")]
        public string Status { get; set; } = "active";

        public string? Notes { get; set; }
    }

    // ==================== UPDATE SUPPLIER DTO ====================
    public class UpdateSupplierDto
    {
        [Required(ErrorMessage = "Mã nhà cung cấp không được để trống")]
        [MaxLength(50, ErrorMessage = "Mã nhà cung cấp không được quá 50 ký tự")]
        public string SupplierCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        [MaxLength(255, ErrorMessage = "Tên nhà cung cấp không được quá 255 ký tự")]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Tên người liên hệ không được quá 255 ký tự")]
        public string? ContactPerson { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        public string? Email { get; set; }

        [MaxLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        public string? Address { get; set; }

        [MaxLength(50, ErrorMessage = "Mã số thuế không được quá 50 ký tự")]
        public string? TaxCode { get; set; }

        [MaxLength(100, ErrorMessage = "Số tài khoản ngân hàng không được quá 100 ký tự")]
        public string? BankAccount { get; set; }

        [MaxLength(255, ErrorMessage = "Tên ngân hàng không được quá 255 ký tự")]
        public string? BankName { get; set; }

        [MaxLength(255, ErrorMessage = "Bảng giá không được quá 255 ký tự")]
        public string? PriceList { get; set; }

        [MaxLength(255, ErrorMessage = "URL logo không được quá 255 ký tự")]
        public string? LogoUrl { get; set; }

        [MaxLength(20, ErrorMessage = "Trạng thái không được quá 20 ký tự")]
        public string Status { get; set; } = "active";

        public string? Notes { get; set; }
    }

    // ==================== SEARCH SUPPLIER DTO ====================
    public class SearchSupplierDto
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        public string? ContactPerson { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // ==================== SUPPLIER LIST RESPONSE DTO ====================
    public class SupplierListResponseDto
    {
        public List<SupplierInfoDto> Suppliers { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // ==================== SUPPLIER STATISTICS DTO ====================
    public class SupplierStatisticsDto
    {
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }
        public int InactiveSuppliers { get; set; }
        public int SuppliersWithProducts { get; set; }
        public int SuppliersWithOrders { get; set; }
    }
}
