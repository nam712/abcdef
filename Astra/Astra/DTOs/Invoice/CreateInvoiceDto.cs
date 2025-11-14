using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.Invoice
{
    /// <summary>
    /// DTO tạo hóa đơn bán hàng mới
    /// </summary>
    public class CreateInvoiceDto
    {
        [Required(ErrorMessage = "Mã hóa đơn không được để trống")]
        [MaxLength(50, ErrorMessage = "Mã hóa đơn không được vượt quá 50 ký tự")]
        public string InvoiceCode { get; set; } = string.Empty;

        public int? ShopId { get; set; }

        /// <summary>
        /// Mã khách hàng - Nullable (cho phép bán lẻ không lưu thông tin khách)
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Mã nhân viên - BẮT BUỘC (phải biết ai bán hàng)
        /// </summary>
        [Required(ErrorMessage = "Nhân viên không được để trống")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Ngày lập hóa đơn không được để trống")]
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá không hợp lệ")]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền đã thanh toán không hợp lệ")]
        public decimal AmountPaid { get; set; } = 0;

        public int? PaymentMethodId { get; set; }

        /// <summary>
        /// Mã khuyến mãi áp dụng (nếu có)
        /// </summary>
        public int? PromotionId { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Phải có ít nhất một sản phẩm trong hóa đơn")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một sản phẩm trong hóa đơn")]
        public List<CreateInvoiceDetailDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO chi tiết sản phẩm trong hóa đơn
    /// </summary>
    public class CreateInvoiceDetailDto
    {
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
        public decimal UnitPrice { get; set; }
    }
}