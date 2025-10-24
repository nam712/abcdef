using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourShopManagement.API.DTOs.PurchaseOrder
{
    /// <summary>
    /// DTO tạo phiếu nhập hàng mới
    /// </summary>
    public class CreatePurchaseOrderDto
    {
        [Required(ErrorMessage = "Mã phiếu nhập không được để trống")]
        [MaxLength(50, ErrorMessage = "Mã phiếu nhập không được vượt quá 50 ký tự")]
        public string PoCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhà cung cấp không được để trống")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Ngày lập phiếu không được để trống")]
        public DateTime PoDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedDeliveryDate { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Phải có ít nhất một sản phẩm trong phiếu nhập")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một sản phẩm trong phiếu nhập")]
        public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO chi tiết sản phẩm trong phiếu nhập
    /// </summary>
    public class CreatePurchaseOrderDetailDto
    {
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Giá nhập không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn 0")]
        public decimal ImportPrice { get; set; }
    }
}