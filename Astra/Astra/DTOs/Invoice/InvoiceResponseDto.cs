using System;
using System.Collections.Generic;

namespace YourShopManagement.API.DTOs.Invoice
{
    /// <summary>
    /// DTO response hóa đơn đầy đủ
    /// </summary>
    public class InvoiceResponseDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        
        // Customer Info
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        
        // Employee Info
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        
        // Payment Method Info
        public int? PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountRemaining { get; set; } // Số tiền còn nợ
        public string PaymentStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public List<InvoiceDetailResponseDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO chi tiết sản phẩm trong hóa đơn
    /// </summary>
    public class InvoiceDetailResponseDto
    {
        public int InvoiceDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// DTO danh sách hóa đơn (cho list/search)
    /// </summary>
    public class InvoiceListDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}