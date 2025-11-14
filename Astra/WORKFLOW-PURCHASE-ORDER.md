# 📦 WORKFLOW: Quản lý Phiếu Nhập Hàng & Sản Phẩm

## 🎯 Mục tiêu
Tránh tạo trùng sản phẩm khi nhập hàng nhiều lần từ cùng một mặt hàng.

---

## 🔄 Luồng nghiệp vụ ĐÚNG

### **Bước 1: Tạo Phiếu Nhập (CreatePurchaseOrderAsync)**

```csharp
// DTO Request
public class CreatePurchaseOrderDetailDto
{
    public string ProductCode { get; set; }      // Mã sản phẩm (bắt buộc)
    public string ProductName { get; set; }      // Tên sản phẩm
    public int? CategoryId { get; set; }         // Danh mục
    public string? Brand { get; set; }           // Thương hiệu
    public string? Unit { get; set; }            // Đơn vị tính
    public string? Barcode { get; set; }         // Mã vạch
    public decimal ImportPrice { get; set; }     // Giá nhập
    public decimal SuggestedPrice { get; set; }  // Giá bán đề xuất
    public int Quantity { get; set; }            // Số lượng
}

// Service Logic
public async Task<PurchaseOrder> CreatePurchaseOrderAsync(
    CreatePurchaseOrderDto dto, 
    int shopOwnerId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. Tạo Purchase Order
        var purchaseOrder = new PurchaseOrder
        {
            ShopId = dto.ShopId,
            PoCode = await GeneratePoCodeAsync(),
            SupplierId = dto.SupplierId,
            PoDate = DateTime.UtcNow,
            Status = "pending",
            PaymentStatus = "unpaid",
            TotalAmount = 0
        };
        _context.PurchaseOrders.Add(purchaseOrder);
        await _context.SaveChangesAsync();

        // 2. Xử lý từng sản phẩm trong phiếu nhập
        decimal totalAmount = 0;
        foreach (var detailDto in dto.Details)
        {
            // ⭐ CHECK: Sản phẩm đã tồn tại chưa?
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => 
                    p.ProductCode == detailDto.ProductCode);

            int productId;

            if (existingProduct != null)
            {
                // ✅ SẢN PHẨM ĐÃ TỒN TẠI → Dùng product_id có sẵn
                productId = existingProduct.ProductId;
                
                // Cập nhật thông tin (nếu cần)
                existingProduct.SupplierName = dto.SupplierName;
                existingProduct.CostPrice = detailDto.ImportPrice;
                existingProduct.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // ❌ SẢN PHẨM CHƯA TỒN TẠI → Tạo mới
                var newProduct = new Product
                {
                    ProductCode = detailDto.ProductCode,
                    ProductName = detailDto.ProductName,
                    CategoryId = detailDto.CategoryId,
                    Brand = detailDto.Brand,
                    SupplierName = dto.SupplierName,
                    Price = detailDto.SuggestedPrice,
                    CostPrice = detailDto.ImportPrice,
                    Stock = 0,  // ⚠️ Chưa tăng stock (chờ received)
                    Unit = detailDto.Unit,
                    Barcode = detailDto.Barcode,
                    Status = "pending_import",  // Chưa active
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();
                
                productId = newProduct.ProductId;
            }

            // 3. Tạo Purchase Order Detail
            var detail = new PurchaseOrderDetail
            {
                PurchaseOrderId = purchaseOrder.PurchaseOrderId,
                ProductId = productId,  // ⭐ Dùng productId (mới hoặc cũ)
                Quantity = detailDto.Quantity,
                ImportPrice = detailDto.ImportPrice,
                FinalAmount = detailDto.Quantity * detailDto.ImportPrice
            };
            _context.PurchaseOrderDetails.Add(detail);
            totalAmount += detail.FinalAmount;
        }

        // 4. Cập nhật tổng tiền
        purchaseOrder.TotalAmount = totalAmount;
        await _context.SaveChangesAsync();
        
        await transaction.CommitAsync();
        return purchaseOrder;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

### **Bước 2: Xác Nhận Nhập Hàng (ConfirmPurchaseOrderAsync)**

```csharp
public async Task ConfirmPurchaseOrderAsync(int purchaseOrderId)
{
    var purchaseOrder = await _context.PurchaseOrders
        .Include(po => po.PurchaseOrderDetails)
            .ThenInclude(pod => pod.Product)
        .FirstOrDefaultAsync(po => po.PurchaseOrderId == purchaseOrderId);

    if (purchaseOrder == null)
        throw new NotFoundException("Phiếu nhập không tồn tại");

    if (purchaseOrder.Status != "pending")
        throw new BadRequestException("Chỉ có thể xác nhận phiếu pending");

    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. Cập nhật stock cho từng sản phẩm
        foreach (var detail in purchaseOrder.PurchaseOrderDetails)
        {
            var product = detail.Product;
            
            // ⭐ TĂNG STOCK
            product.Stock += detail.Quantity;
            
            // Cập nhật giá vốn
            product.CostPrice = detail.ImportPrice;
            
            // Active sản phẩm nếu đang pending
            if (product.Status == "pending_import")
            {
                product.Status = "active";
            }
            
            product.UpdatedAt = DateTime.UtcNow;
        }

        // 2. Đổi trạng thái phiếu nhập
        purchaseOrder.Status = "received";
        purchaseOrder.ActualDeliveryDate = DateTime.UtcNow;
        purchaseOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

### **Bước 3: GET Products API (Chỉ hiển thị hàng có sẵn)**

```csharp
public async Task<List<ProductDto>> GetActiveProductsAsync(int shopOwnerId)
{
    var products = await _context.Products
        .Where(p => 
            p.Status == "active" &&     // Đã được nhập hàng ít nhất 1 lần
            p.Stock > 0)                 // Còn hàng trong kho
        .Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            ProductCode = p.ProductCode,
            ProductName = p.ProductName,
            Price = p.Price,
            Stock = p.Stock,
            // ...
        })
        .ToListAsync();

    return products;
}
```

---

## 📊 Ví dụ Minh Họa

### **Timeline: Nhập "Chuối" 3 lần**

| Thời gian | Hành động | products table | purchase_order_details |
|-----------|-----------|----------------|------------------------|
| **T1** | Nhập lần 1: Chuối (50 quả) từ NCC A | `product_id=1, code="CH001", name="Chuối", stock=0` (pending) | `product_id=1, qty=50` |
| **T2** | Xác nhận nhập | `product_id=1, stock=50, status="active"` | - |
| **T3** | Bán hết 50 quả | `product_id=1, stock=0` | - |
| **T4** | Nhập lần 2: Chuối (100 quả) từ NCC A | ✅ **Tìm thấy CH001 → Dùng product_id=1** | `product_id=1, qty=100` |
| **T5** | Xác nhận nhập | `product_id=1, stock=100` | - |
| **T6** | Nhập lần 3: Chuối (200 quả) từ NCC B | ✅ **Vẫn dùng product_id=1** (cùng code) | `product_id=1, qty=200` |
| **T7** | Xác nhận nhập | `product_id=1, stock=300` | - |

**Kết quả:** Chỉ có **1 sản phẩm "Chuối"** với `product_id=1` xuyên suốt!

---

## 🔑 Key Points

1. **UNIQUE product_code**: Đảm bảo không tạo trùng
2. **Check existing trước khi tạo mới**: Luôn kiểm tra `product_code` đã tồn tại chưa
3. **Stock chỉ tăng khi confirm**: Không tăng ngay khi tạo phiếu
4. **Status pending_import**: Sản phẩm chưa được nhập lần nào
5. **GET chỉ lấy active + stock > 0**: Không hiển thị hàng chưa nhập hoặc hết hàng

---

## ⚠️ Lưu ý quan trọng

### **Nếu cùng sản phẩm nhưng khác NCC:**
```csharp
// BAD ❌: Dùng supplier_id trong products
// → Sẽ tạo 2 product cho cùng 1 hàng

// GOOD ✅: Chỉ lưu supplier_name (từ lần nhập gần nhất)
product.SupplierName = currentSupplier.SupplierName;
```

### **Nếu muốn theo dõi giá từng NCC:**
Tạo bảng riêng:
```sql
CREATE TABLE product_supplier_prices (
    id SERIAL PRIMARY KEY,
    product_id INT NOT NULL,
    supplier_id INT NOT NULL,
    price DECIMAL(18,2) NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT uq_product_supplier UNIQUE (product_id, supplier_id)
);
```

---

## 🎯 Tóm tắt

✅ **Database hiện tại của bạn ĐÃ ĐÚNG** (có FK products ← purchase_order_details)  
✅ **Logic cần sửa:** Backend phải check `product_code` trước khi INSERT  
✅ **Workflow:** Create phiếu → Check/Create product → Confirm → Tăng stock  
✅ **GET API:** Chỉ hiển thị `status='active' AND stock > 0`

---

## 📝 Next Steps

Bạn cần sửa:
1. **PurchaseOrderService.cs** → Thêm logic check existing product
2. **ProductsController.cs** → Xóa POST endpoint (chỉ giữ GET)
3. **ApplicationDbContext.cs** → Thêm unique index cho product_code

Bạn có muốn tôi sửa code service ngay không?
