
namespace YourShopManagement.API.DTOs.PurchaseOrder
{
    public class CreatePurchaseOrderDto
    {
        public string PoCode { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public DateTime PoDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
    }

    public class CreatePurchaseOrderDetailDto
    {
        // ⭐ Thông tin sản phẩm (để tạo mới nếu chưa tồn tại)
        public string ProductCode { get; set; } = string.Empty;      // Mã sản phẩm (bắt buộc - unique)
        public string ProductName { get; set; } = string.Empty;      // Tên sản phẩm
        public int? CategoryId { get; set; }                         // Danh mục
        public string? Brand { get; set; }                           // Thương hiệu
        public string? Unit { get; set; }                            // Đơn vị tính (cái, kg, hộp...)
        public string? Barcode { get; set; }                         // Mã vạch
        public string? Sku { get; set; }                             // SKU
        public string? ImageUrl { get; set; }                        // URL ảnh sản phẩm
        public decimal? Weight { get; set; }                         // Cân nặng
        public string? Dimension { get; set; }                       // Kích thước
        public decimal SuggestedPrice { get; set; }                  // Giá bán đề xuất
        
        // ⭐ Thông tin phiếu nhập
        public int Quantity { get; set; }                            // Số lượng nhập
        public decimal ImportPrice { get; set; }                     // Giá nhập
    }
}