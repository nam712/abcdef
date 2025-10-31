namespace YourShopManagement.API.DTOs.PurchaseOrder
{
    public class PurchaseOrderDto
    {
        public int PurchaseOrderId { get; set; }
        public string PoCode { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public DateTime PoDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string? Notes { get; set; }

        public List<PurchaseOrderDetailDto> Details { get; set; } = new();
    }

    public class PurchaseOrderDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal FinalAmount { get; set; }
    }
}