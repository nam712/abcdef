using System;
using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.Invoice
{
    /// <summary>
    /// DTO cập nhật hóa đơn
    /// </summary>
    public class UpdateInvoiceDto
    {
        /// <summary>
        /// Mã khách hàng - Nullable (cho phép bán lẻ)
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Mã nhân viên - BẮT BUỘC
        /// </summary>
        [Required(ErrorMessage = "Nhân viên không được để trống")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Ngày lập hóa đơn không được để trống")]
        public DateTime InvoiceDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá không hợp lệ")]
        public decimal DiscountAmount { get; set; } = 0;

        public int? PaymentMethodId { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO thanh toán hóa đơn
    /// </summary>
    public class PaymentInvoiceDto
    {
        [Required(ErrorMessage = "Phương thức thanh toán không được để trống")]
        public string PaymentType { get; set; } = string.Empty; // "cash" hoặc "momo"

        [Required(ErrorMessage = "Số tiền thanh toán không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền thanh toán phải lớn hơn 0")]
        public decimal Amount { get; set; }
    }
}