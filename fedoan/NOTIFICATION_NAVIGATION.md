# Tính năng Click Notification để Navigate

## 🎯 Mô tả

Khi click vào thông báo, hệ thống sẽ tự động:
1. **Đánh dấu đã đọc** (nếu chưa đọc)
2. **Chuyển đến trang liên quan** (nếu có route)
3. **Đóng dropdown** thông báo

## 📋 Các loại notification và route tương ứng

| Entity Type | Route | Mô tả |
|------------|-------|-------|
| Product | `/products` | Trang quản lý sản phẩm |
| Customer | `/customers` | Trang quản lý khách hàng |
| Employee | `/employees` | Trang quản lý nhân viên |
| Supplier | `/manufacturer` | Trang quản lý nhà cung cấp |
| Invoice | `/pos` | Trang POS/Bán hàng |
| PurchaseOrder | `/purchase-orders` | Trang đơn mua hàng |

## 🎨 UI/UX

### Visual Indicators

**Thông báo CÓ thể click:**
- ✅ Cursor: `pointer`
- ✅ Hover effect: Nền xanh nhạt + dịch sang phải
- ✅ Icon hint: "→ Click để xem" (màu xanh)
- ✅ Tooltip: "Click để xem chi tiết"

**Thông báo KHÔNG thể click:**
- ❌ Cursor: `default`
- ❌ Không có hover effect đặc biệt
- ❌ Không có icon hint

### CSS Classes

```css
.notification-item.clickable {
  cursor: pointer;
}

.notification-item.clickable:hover {
  background: #f0f9ff;
  transform: translateX(2px);
}

.notification-link-hint {
  color: #3b82f6;
  font-size: 11px;
  font-weight: 500;
}
```

## 💻 Code Examples

### 1. Tạo notification với route

```typescript
// Với route cụ thể
this.notificationService.addNotification(
  'Đã thêm sản phẩm "Coca Cola" thành công!',
  'success',
  {
    entityType: 'Product',
    entityId: 123,
    action: 'Create',
    route: '/products'  // ← Thêm route
  }
);

// Route tự động từ entityType
this.notificationService.addNotification(
  'Khách hàng mới đã được tạo!',
  'success',
  {
    entityType: 'Customer',  // ← Route tự động = /customers
    entityId: 456,
    action: 'Create'
  }
);
```

### 2. Xử lý click trong component

```typescript
onNotificationClick(notification: Notification): void {
  // Auto đánh dấu đã đọc
  if (!notification.isRead) {
    this.notificationService.markAsRead(notification.id);
  }
  
  // Navigate nếu có route
  if (notification.route) {
    this.router.navigate([notification.route]);
    this.closeNotifications();
  }
}
```

### 3. Custom route cho trường hợp đặc biệt

```typescript
// Navigate đến sản phẩm cụ thể (với ID)
this.notificationService.addNotification(
  'Sản phẩm sắp hết hàng!',
  'warning',
  {
    entityType: 'Product',
    entityId: productId,
    route: `/products/edit/${productId}`  // Custom route
  }
);
```

## 🔧 Auto-Generate Route

Service tự động tạo route từ `entityType`:

```typescript
private generateRoute(entityType: string, entityId: number | string): string {
  const routes: { [key: string]: string } = {
    'Product': '/products',
    'Customer': '/customers',
    'Employee': '/employees',
    'Supplier': '/manufacturer',
    'Invoice': '/pos',
    'PurchaseOrder': '/purchase-orders'
  };
  
  return routes[entityType] || '/dashboard';
}
```

**Fallback:** Nếu không tìm thấy entityType → Navigate đến `/dashboard`

## 📊 Database Schema

Route không lưu trong database (chỉ tính toán runtime từ entityType).

**Lưu:**
- ✅ `entityType` (string)
- ✅ `entityId` (number/string)
- ✅ `action` (string)

**Tính toán runtime:**
- 🔄 `route` (từ entityType)

## ✅ Test Cases

### Test 1: Click thông báo có route
```
1. Tạo sản phẩm mới
2. Xem thông báo "Đã thêm sản phẩm..."
3. Click vào thông báo
4. ✅ Chuyển đến /products
5. ✅ Thông báo được đánh dấu đã đọc
6. ✅ Dropdown đóng lại
```

### Test 2: Click thông báo KHÔNG có route
```
1. Tạo notification thủ công không có route
2. Click vào thông báo
3. ✅ Thông báo được đánh dấu đã đọc
4. ✅ Dropdown KHÔNG đóng
5. ✅ KHÔNG navigate
```

### Test 3: Visual indicators
```
1. Hover vào notification CÓ route
   ✅ Cursor pointer
   ✅ Background xanh nhạt
   ✅ Dịch sang phải 2px
   ✅ Hiển thị "→ Click để xem"

2. Hover vào notification KHÔNG route
   ✅ Cursor default
   ✅ Background xám nhạt
   ✅ Không dịch chuyển
   ✅ KHÔNG hiển thị hint
```

### Test 4: Inventory alert navigation
```
1. Sản phẩm sắp hết hàng (stock = minStock)
2. Nhận thông báo ⚠️
3. Click thông báo
4. ✅ Navigate đến /products
5. ✅ Có thể tìm sản phẩm đó để nhập hàng
```

## 🎯 Use Cases

### 1. Quản lý tồn kho
```
Notification: "⚠️ Sản phẩm X sắp hết hàng"
→ Click → /products
→ Tìm sản phẩm X
→ Nhập hàng
```

### 2. Quản lý đơn hàng
```
Notification: "✅ Đơn hàng #123 đã hoàn thành"
→ Click → /pos
→ Xem chi tiết đơn
→ In hóa đơn
```

### 3. Quản lý khách hàng
```
Notification: "👤 Khách hàng mới đã đăng ký"
→ Click → /customers
→ Xem thông tin khách hàng
→ Liên hệ/Tư vấn
```

### 4. Quản lý nhà cung cấp
```
Notification: "✅ Đã thêm nhà cung cấp Y"
→ Click → /manufacturer
→ Xem danh sách nhà cung cấp
→ Tạo đơn đặt hàng
```

## 🚀 Tương lai

### Planned Features
- [ ] Navigate kèm query params (filter/search)
- [ ] Navigate đến tab cụ thể trong page
- [ ] Navigate đến modal edit (mở popup trực tiếp)
- [ ] Highlight row/item sau khi navigate
- [ ] Scroll to element sau khi navigate

### Example: Navigate với query params
```typescript
route: `/products?search=${productCode}&highlight=true`
```

### Example: Navigate đến modal
```typescript
route: `/products?action=edit&id=${productId}`
// Component tự động mở modal edit
```

## 📝 Notes

1. **Route phải có trong Angular routing** - Nếu route không tồn tại → 404
2. **Route có thể custom** - Không bắt buộc dùng auto-generate
3. **Dropdown tự động đóng** - Chỉ khi navigate thành công
4. **Notification vẫn tồn tại** - Sau khi click (không xóa)
5. **History được lưu** - User có thể back về page trước

## 🔍 Debug

### Console logs
```typescript
// Khi click notification
console.log('Navigating to:', notification.route);

// Khi không có route
console.log('No route defined for notification:', notification.id);
```

### Check route trong DevTools
```typescript
// Xem notification object
console.log(notification);
// {
//   id: 1,
//   message: "...",
//   route: "/products",  ← Check this
//   entityType: "Product",
//   entityId: 123
// }
```
