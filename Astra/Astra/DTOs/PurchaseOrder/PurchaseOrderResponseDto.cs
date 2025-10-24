using System;
using System.Collections.Generic;

namespace YourShopManagement.API.DTOs.PurchaseOrder
{
    /// <summary>
    /// DTO response phiếu nhập hàng
    /// </summary>
    public class PurchaseOrderResponseDto
    {
        public int PurchaseOrderId { get; set; }
        public string PoCode { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierPhone { get; set; }
        public DateTime PoDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public List<PurchaseOrderDetailResponseDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO chi tiết sản phẩm trong phiếu nhập
    /// </summary>
    public class PurchaseOrderDetailResponseDto
    {
        public int PurchaseOrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal FinalAmount { get; set; }
    }

    /// <summary>
    /// DTO danh sách phiếu nhập (cho list/search)
    /// </summary>
    public class PurchaseOrderListDto
    {
        public int PurchaseOrderId { get; set; }
        public string PoCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime PoDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}