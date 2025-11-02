# Hệ thống Thông báo Toàn cục (Global Notification System)

## 📋 Tổng quan
Hệ thống thông báo toàn cục cho phép hiển thị thông báo từ bất kỳ component nào trong ứng dụng, với icon chuông ở header và lưu trữ lịch sử thông báo.

## 🗂️ Cấu trúc File

```
src/app/
├── services/
│   └── notification.service.ts          # Service quản lý thông báo toàn cục
├── shared/
│   └── notification-bell/
│       ├── notification-bell.component.ts
│       ├── notification-bell.component.html
│       └── notification-bell.component.css
```

## 🚀 Cách sử dụng

### 1. Thêm NotificationBellComponent vào template

Trong file HTML của component (thường là trong header):

```html
<div class="user-section">
  <button class="theme-toggle" (click)="changeTheme()">
    <i class="fas fa-palette"></i>
  </button>
  
  <!-- Thêm notification bell component -->
  <app-notification-bell></app-notification-bell>
  
  <div class="user-profile">
    <!-- User info -->
  </div>
</div>
```

### 2. Import component và service

Trong file TypeScript của component:

```typescript
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-your-component',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    NotificationBellComponent  // ✅ Thêm vào imports
  ],
  templateUrl: './your-component.html',
  styleUrls: ['./your-component.css']
})
export class YourComponent {
  constructor(
    private notificationService: NotificationService  // ✅ Inject service
  ) {}
}
```

### 3. Gửi thông báo

Sử dụng NotificationService để gửi thông báo:

```typescript
// Thông báo thành công
this.notificationService.addNotification('Đã lưu thành công!', 'success');

// Thông báo lỗi
this.notificationService.addNotification('Có lỗi xảy ra!', 'error');

// Thông báo cảnh báo
this.notificationService.addNotification('Vui lòng kiểm tra lại!', 'warning');

// Thông báo thông tin
this.notificationService.addNotification('Có thông tin mới', 'info');
```

### 4. Ví dụ trong các action

```typescript
// Khi thêm sản phẩm
addProduct() {
  this.productService.create(this.newProduct).subscribe({
    next: (response) => {
      this.notificationService.addNotification(
        `Đã thêm sản phẩm ${this.newProduct.name}`, 
        'success'
      );
      this.loadProducts();
    },
    error: (error) => {
      this.notificationService.addNotification(
        'Không thể thêm sản phẩm. Vui lòng thử lại!', 
        'error'
      );
    }
  });
}

// Khi xóa khách hàng
deleteCustomer(customer: Customer) {
  this.customerService.delete(customer.id).subscribe({
    next: () => {
      this.notificationService.addNotification(
        `Đã xóa khách hàng ${customer.name}`, 
        'success'
      );
      this.loadCustomers();
    },
    error: () => {
      this.notificationService.addNotification(
        'Không thể xóa khách hàng!', 
        'error'
      );
    }
  });
}

// Khi cập nhật thông tin
updateProfile() {
  if (!this.validateForm()) {
    this.notificationService.addNotification(
      'Vui lòng điền đầy đủ thông tin!', 
      'warning'
    );
    return;
  }
  
  this.userService.update(this.profile).subscribe({
    next: () => {
      this.notificationService.addNotification(
        'Đã cập nhật thông tin thành công!', 
        'success'
      );
    }
  });
}
```

## 🎨 Loại thông báo

| Type | Màu sắc | Icon | Sử dụng |
|------|---------|------|---------|
| `success` | Xanh lá | ✓ | Thao tác thành công |
| `error` | Đỏ | ✕ | Lỗi, thất bại |
| `warning` | Cam | ⚠ | Cảnh báo, cần chú ý |
| `info` | Xanh dương | ℹ | Thông tin chung |

## 📦 Tính năng

### NotificationService

- ✅ **addNotification()** - Thêm thông báo mới
- ✅ **markAsRead()** - Đánh dấu thông báo đã đọc
- ✅ **markAllAsRead()** - Đánh dấu tất cả đã đọc
- ✅ **clearAll()** - Xóa tất cả thông báo
- ✅ **getNotifications()** - Observable danh sách thông báo
- ✅ **getUnreadCount()** - Observable số lượng chưa đọc

### NotificationBellComponent

- 🔔 Icon chuông với badge số lượng chưa đọc
- 📜 Dropdown hiển thị danh sách thông báo
- ⏱️ Hiển thị thời gian tương đối (vừa xong, 5 phút trước, ...)
- 💾 Lưu trữ trong localStorage (giữ lại khi reload)
- 🔄 Tự động cập nhật thời gian mỗi phút
- 🎯 Giới hạn tối đa 50 thông báo
- 📱 Responsive design

## 🎯 Components đã tích hợp

Hệ thống thông báo đã được tích hợp vào các component sau:

- ✅ **Dashboard** - Trang chủ
- ✅ **Products** - Quản lý sản phẩm
- ✅ **Customers** - Quản lý khách hàng
- ✅ **Employees** - Quản lý nhân viên
- ✅ **Manufacturer** - Quản lý nhà sản xuất
- ✅ **POS** - Bán hàng

## 💡 Best Practices

### 1. Thông báo ngắn gọn
```typescript
// ✅ Good
this.notificationService.addNotification('Đã lưu thành công!', 'success');

// ❌ Avoid
this.notificationService.addNotification(
  'Hệ thống đã tiến hành lưu dữ liệu của bạn vào cơ sở dữ liệu một cách thành công và hoàn tất.', 
  'success'
);
```

### 2. Sử dụng đúng loại thông báo
```typescript
// ✅ Success - cho thao tác thành công
this.notificationService.addNotification('Đã thêm sản phẩm!', 'success');

// ✅ Error - cho lỗi thực sự
this.notificationService.addNotification('Không thể kết nối server!', 'error');

// ✅ Warning - cho cảnh báo
this.notificationService.addNotification('Sản phẩm sắp hết hàng!', 'warning');

// ✅ Info - cho thông tin chung
this.notificationService.addNotification('Có 3 đơn hàng mới', 'info');
```

### 3. Thông báo có ý nghĩa
```typescript
// ✅ Good - Cụ thể, rõ ràng
this.notificationService.addNotification('Đã xóa khách hàng Nguyễn Văn A', 'success');

// ❌ Avoid - Quá chung chung
this.notificationService.addNotification('Thành công', 'success');
```

## 🔧 Tùy chỉnh

### Thay đổi giới hạn thông báo
Trong `notification.service.ts`:
```typescript
// Thay đổi từ 50 thành số khác
if (updatedNotifications.length > 100) {
  updatedNotifications.splice(100);
}
```

### Thay đổi thời gian cập nhật
Trong `notification.service.ts`:
```typescript
// Thay đổi từ 60000ms (1 phút) thành thời gian khác
setTimeout(() => {
  // ...
}, 120000); // 2 phút
```

## 📱 Responsive

Notification bell tự động responsive:
- Desktop: Dropdown width 380px
- Mobile: Dropdown width 100vw - 32px

## 🎨 Customization CSS

Có thể tùy chỉnh màu sắc trong `notification-bell.component.css`:

```css
/* Badge màu đỏ */
.notification-badge {
  background: #ef4444;
}

/* Icon thông báo success */
.notification-icon-success {
  background: #d1fae5;
  color: #10b981;
}
```

## 🐛 Troubleshooting

### Lỗi: 'app-notification-bell' is not a known element
**Giải pháp:** Import `NotificationBellComponent` vào `@Component.imports`

### Thông báo không hiển thị
**Giải pháp:** 
1. Kiểm tra đã inject `NotificationService` chưa
2. Kiểm tra console log có lỗi không
3. Kiểm tra đã thêm `<app-notification-bell>` vào template chưa

### Badge không cập nhật
**Giải pháp:** Service sử dụng RxJS Observable, đảm bảo component đã subscribe

## 📚 API Reference

### NotificationService

```typescript
interface Notification {
  id: number;
  message: string;
  type: 'info' | 'warning' | 'success' | 'error';
  time: string;
  isRead: boolean;
  timestamp: Date;
}

class NotificationService {
  // Thêm thông báo mới
  addNotification(message: string, type: 'info' | 'warning' | 'success' | 'error'): void

  // Đánh dấu 1 thông báo đã đọc
  markAsRead(notificationId: number): void

  // Đánh dấu tất cả đã đọc
  markAllAsRead(): void

  // Xóa tất cả thông báo
  clearAll(): void

  // Observable danh sách thông báo
  getNotifications(): Observable<Notification[]>

  // Observable số lượng chưa đọc
  getUnreadCount(): Observable<number>
}
```

---

💡 **Tip:** Hãy sử dụng thông báo một cách có chọn lọc để không làm người dùng bị overwhelm!
