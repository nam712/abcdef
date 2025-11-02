# Hệ thống Cảnh báo Tồn kho (Inventory Alert System)

## 🎯 Tính năng

Tự động cảnh báo khi:
- ✅ Số lượng sản phẩm **= Tồn kho tối thiểu** → Thông báo **WARNING** (màu vàng)
- ✅ Số lượng sản phẩm **< Tồn kho tối thiểu** → Thông báo **WARNING** (màu vàng)  
- ✅ Số lượng sản phẩm **= 0** → Thông báo **ERROR** (màu đỏ)

## 📋 Cách hoạt động

### 1. **Kiểm tra tự động**
- Hệ thống kiểm tra tồn kho **mỗi 60 giây**
- Bắt đầu tự động khi app khởi động
- Chạy ở background không ảnh hưởng hiệu suất

### 2. **Kiểm tra theo sự kiện**
- Khi **tạo sản phẩm mới** → Kiểm tra ngay
- Khi **cập nhật số lượng** → Kiểm tra ngay
- Khi **nhập/xuất hàng** → Kiểm tra ngay

### 3. **Tránh spam thông báo**
- Mỗi sản phẩm chỉ cảnh báo **1 lần trong 24 giờ**
- Nếu nhập thêm hàng (stock > minStock) → Cho phép cảnh báo lại

## 📊 Loại thông báo

### ⚠️ Sắp hết hàng (Low Stock)
```
⚠️ Sản phẩm "Coca Cola" sắp hết hàng! (Còn 10 / Tối thiểu 10)
```
- **Type**: `warning` (màu vàng)
- **Action**: `LowStock`
- **Metadata**: Lưu thông tin chi tiết sản phẩm

### 🚨 Hết hàng (Out of Stock)
```
🚨 Sản phẩm "Coca Cola" đã hết hàng! Cần nhập hàng ngay.
```
- **Type**: `error` (màu đỏ)
- **Action**: `OutOfStock`
- **Metadata**: Lưu thông tin chi tiết sản phẩm

## 🔧 Cấu hình

### Thay đổi tần suất kiểm tra
File: `inventory-alert.service.ts`
```typescript
private checkInterval = 60000; // 60 giây (mặc định)
// Có thể đổi thành:
// 30000  = 30 giây (kiểm tra nhanh hơn)
// 300000 = 5 phút (tiết kiệm tài nguyên)
```

### Tắt/Bật monitoring
```typescript
// Tắt
this.inventoryAlertService.stopMonitoring();

// Bật lại
this.inventoryAlertService.startMonitoring();
```

### Reset danh sách đã cảnh báo
```typescript
this.inventoryAlertService.resetAlerts();
```

## 📝 Ví dụ sử dụng

### 1. Kiểm tra một sản phẩm cụ thể
```typescript
const product = {
  productId: 123,
  productName: "Coca Cola",
  stock: 5,
  minimumStock: 10,
  productCode: "CC-001"
};

this.inventoryAlertService.checkProduct(product);
```

### 2. Trong POS khi bán hàng
```typescript
// Sau khi bán hàng, giảm số lượng
product.stock -= soldQuantity;

// Kiểm tra tồn kho
this.inventoryAlertService.checkProduct(product);
```

### 3. Trong Purchase Order khi nhập hàng
```typescript
// Sau khi nhập hàng, tăng số lượng
product.stock += importedQuantity;

// Kiểm tra (sẽ reset cảnh báo nếu đủ hàng)
this.inventoryAlertService.checkProduct(product);
```

## 🗄️ Dữ liệu lưu trong Database

Thông báo sẽ được lưu vào bảng `Notifications`:

| Field | Giá trị | Mô tả |
|-------|---------|-------|
| Message | "⚠️ Sản phẩm..." | Nội dung cảnh báo |
| Type | "warning" hoặc "error" | Loại thông báo |
| EntityType | "Product" | Loại entity |
| EntityId | 123 | ID sản phẩm |
| Action | "LowStock" hoặc "OutOfStock" | Hành động |
| UserId | "user123" | ID người dùng (tự động) |
| Metadata | JSON | Thông tin chi tiết |

**Metadata JSON:**
```json
{
  "productCode": "CC-001",
  "productName": "Coca Cola",
  "currentStock": 5,
  "minimumStock": 10
}
```

## 🎨 Hiển thị trên UI

Thông báo sẽ xuất hiện ở:
1. **Icon chuông 🔔** → Badge tăng lên
2. **Dropdown notifications** → Màu vàng (warning) hoặc đỏ (error)
3. **Console log** → `⚠️ Cảnh báo tồn kho thấp: Coca Cola`

## ✅ Test

### Test Case 1: Sản phẩm sắp hết hàng
1. Tạo sản phẩm với `stock = 10`, `minStock = 10`
2. → Sẽ thấy thông báo ⚠️ màu vàng
3. Check database: `Action = 'LowStock'`

### Test Case 2: Sản phẩm hết hàng
1. Tạo sản phẩm với `stock = 0`, `minStock = 10`
2. → Sẽ thấy thông báo 🚨 màu đỏ
3. Check database: `Action = 'OutOfStock'`

### Test Case 3: Nhập hàng → Reset cảnh báo
1. Sản phẩm đang `stock = 5`, `minStock = 10` (đã cảnh báo)
2. Nhập thêm 10 → `stock = 15`
3. → Cho phép cảnh báo lại nếu sau này lại xuống thấp

### Test Case 4: Không spam
1. Sản phẩm `stock = 5`, `minStock = 10` → Cảnh báo lần 1 ✅
2. Đợi 5 giây → Không cảnh báo lại ❌
3. Đợi thêm 24 giờ → Cảnh báo lần 2 ✅

## 🔥 Tips

1. **Xem log realtime:**
   - Mở F12 Console
   - Sẽ thấy: `⚠️ Cảnh báo tồn kho thấp: [Tên sản phẩm]`

2. **Kiểm tra database:**
   ```sql
   SELECT * FROM "Notifications" 
   WHERE "Action" IN ('LowStock', 'OutOfStock')
   ORDER BY "CreatedAt" DESC;
   ```

3. **Tối ưu hiệu suất:**
   - Tăng `checkInterval` nếu có nhiều sản phẩm
   - Thêm pagination cho API `GET /api/Products`

## 🎯 Tương lai

- [ ] Email notification khi sản phẩm hết hàng
- [ ] SMS notification cho quản lý
- [ ] Dashboard widget hiển thị sản phẩm sắp hết
- [ ] Auto-generate Purchase Order
- [ ] Báo cáo tồn kho theo thời gian
