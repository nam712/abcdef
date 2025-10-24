using System;
using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.PurchaseOrder
{
    /// <summary>
    /// DTO cập nhật phiếu nhập hàng
    /// </summary>
    public class UpdatePurchaseOrderDto
    {
        [Required(ErrorMessage = "Nhà cung cấp không được để trống")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Ngày lập phiếu không được để trống")]
        public DateTime PoDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        [MaxLength(20, ErrorMessage = "Trạng thái không hợp lệ")]
        [RegularExpression("^(pending|received|cancelled)$", ErrorMessage = "Trạng thái phải là: pending, received, hoặc cancelled")]
        public string? Status { get; set; }

        [MaxLength(20, ErrorMessage = "Trạng thái thanh toán không hợp lệ")]
        [RegularExpression("^(unpaid|partial|paid)$", ErrorMessage = "Trạng thái thanh toán phải là: unpaid, partial, hoặc paid")]
        public string? PaymentStatus { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO xác nhận nhận hàng
    /// </summary>
    public class ReceivePurchaseOrderDto
    {
        [Required(ErrorMessage = "Ngày nhận hàng không được để trống")]
        public DateTime ActualDeliveryDate { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }
    }
}