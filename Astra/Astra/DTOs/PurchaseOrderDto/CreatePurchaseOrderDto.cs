
namespace YourShopManagement.API.DTOs.PurchaseOrder
{
    public class CreatePurchaseOrderDto
    {
        public string PoCode { get; set; }
        public int SupplierId { get; set; }
        public DateTime PoDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
    }

    public class CreatePurchaseOrderDetailDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
    }
}