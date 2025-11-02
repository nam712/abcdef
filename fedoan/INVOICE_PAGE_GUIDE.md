# 📋 Trang Chi tiết Hóa đơn - Hướng dẫn sử dụng

## ✅ Đã tạo thành công!

Trang **Chi tiết Hóa đơn** đã được tạo thành công với đầy đủ các tính năng giống như trang Nhà sản xuất.

## 📁 Các file đã tạo:

### 1. **Model** - `invoice.model.ts`
- Định nghĩa interface cho Invoice và InvoiceDetail
- Chứa các properties: invoiceCode, customerId, totalAmount, paymentStatus, v.v.
- Location: `fedoan/src/app/models/invoice.model.ts`

### 2. **Service** - `invoice.service.ts`
- Các phương thức API: getAllInvoices, getInvoiceById, createInvoice, updateInvoice, deleteInvoice
- Xử lý authentication với JWT token
- Location: `fedoan/src/app/services/invoice.service.ts`

### 3. **Component TypeScript** - `invoice.component.ts`
- Quản lý state và logic của trang
- Xử lý filters: tìm kiếm, trạng thái thanh toán, khoảng ngày, khoảng giá
- CRUD operations: Create, Read, Update, Delete
- Tích hợp notification service
- Location: `fedoan/src/app/invoice/invoice.component.ts`

### 4. **Component HTML** - `invoice.component.html`
- Giao diện responsive với sidebar filters
- Bảng hiển thị danh sách hóa đơn
- Dialog thêm/sửa hóa đơn
- Navigation bar đầy đủ
- Location: `fedoan/src/app/invoice/invoice.component.html`

### 5. **Component CSS** - `invoice.component.css`
- Import styles từ manufacturer component
- Custom styles cho payment status badges
- Responsive design
- Location: `fedoan/src/app/invoice/invoice.component.css`

### 6. **Routing** - `app-routing.module.ts`
- Đã thêm route: `/invoices` → InvoiceComponent
- Location: `fedoan/src/app/app-routing.module.ts`

## 🎯 Các tính năng chính:

### ✨ Filters (Bộ lọc):
- 🔍 Tìm kiếm theo: Mã HĐ, khách hàng, nhân viên, ghi chú
- 💰 Lọc theo trạng thái thanh toán: Đã thanh toán, Chưa thanh toán, Thanh toán 1 phần, Đã hoàn tiền
- 📅 Lọc theo khoảng thời gian (Từ ngày - Đến ngày)
- 💵 Lọc theo khoảng giá (Số tiền từ - Số tiền đến)
- 🔄 Sắp xếp: Ngày mới/cũ nhất, Số tiền cao/thấp nhất, Mã A-Z/Z-A

### 📊 Hiển thị dữ liệu:
- Bảng danh sách với các cột: Mã HĐ, Khách hàng, Nhân viên, Ngày tạo, Tổng tiền, Giảm giá, Thành tiền, Trạng thái
- Format tiền tệ VND
- Format ngày tháng theo locale Việt Nam
- Status badges với màu sắc phân biệt

### 🛠️ CRUD Operations:
- ➕ **Thêm mới**: Dialog với form đầy đủ các trường
- 👁️ **Xem chi tiết**: Hiển thị thông tin đầy đủ
- ✏️ **Chỉnh sửa**: Cập nhật thông tin hóa đơn
- 🗑️ **Xóa**: Xác nhận trước khi xóa

### 📱 Responsive Design:
- Desktop: Sidebar cố định
- Mobile: Sidebar trượt từ bên trái
- Overlay khi mở filters trên mobile
- Nút toggle filters trên mobile

### 🔔 Notifications:
- Tích hợp NotificationService
- Thông báo khi thêm/sửa/xóa thành công
- Thông báo lỗi khi có vấn đề

### 🎨 Theme Support:
- 5 theme colors: Sáng, Tối, Xanh dương, Xanh lá, Tím
- Lưu theme preference vào localStorage
- Áp dụng theme toàn bộ trang

## 🚀 Cách sử dụng:

### 1. Truy cập trang:
```
http://localhost:4200/invoices
```

### 2. Hoặc thêm link trong navigation:
Đã có link "Hóa đơn" trong navigation bar của trang

### 3. API Backend cần có:
Trang này kết nối đến API endpoint: `http://localhost:5001/api/Invoice`

API cần có các endpoint:
- `GET /api/Invoice` - Lấy danh sách
- `GET /api/Invoice/{id}` - Lấy chi tiết
- `POST /api/Invoice` - Tạo mới
- `PUT /api/Invoice/{id}` - Cập nhật
- `DELETE /api/Invoice/{id}` - Xóa

## 🔧 Cấu hình API:

File `environment.ts` đã được sử dụng:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5001'
};
```

## 📝 Cấu trúc Invoice Model:

```typescript
interface Invoice {
  invoiceId?: number;
  invoiceCode: string;           // Mã HĐ (auto-generated)
  customerId: number;             // ID khách hàng
  employeeId?: number | null;     // ID nhân viên
  invoiceDate: Date | string;     // Ngày tạo
  totalAmount: number;            // Tổng tiền
  discountAmount?: number;        // Giảm giá
  finalAmount: number;            // Thành tiền
  amountPaid?: number;            // Đã thanh toán
  paymentMethodId?: number | null;// ID phương thức TT
  paymentStatus: string;          // Trạng thái: paid/unpaid/partial/refunded
  notes?: string;                 // Ghi chú
  
  // Navigation properties (từ API)
  customerName?: string;
  employeeName?: string;
  paymentMethodName?: string;
  invoiceDetails?: InvoiceDetail[];
}
```

## 🎨 Payment Status:

| Status    | Label                | Color        |
|-----------|---------------------|--------------|
| `paid`    | Đã thanh toán       | Green        |
| `unpaid`  | Chưa thanh toán     | Red          |
| `partial` | Thanh toán 1 phần   | Yellow       |
| `refunded`| Đã hoàn tiền        | Blue         |

## 💡 Tips:

1. **Auto-generate Invoice Code**: Mã hóa đơn tự động theo format `INV{YY}{MM}{XXXX}`
   - YY: 2 số cuối năm
   - MM: Tháng (2 chữ số)
   - XXXX: Random 4 chữ số

2. **Filter Summary**: Hiển thị số lượng hóa đơn sau khi filter

3. **Mobile Friendly**: Filters có thể đóng/mở trên mobile, có overlay

4. **Error Handling**: Xử lý đầy đủ các lỗi API: 401, 404, 400, 0 (connection failed)

## 🔗 Navigation:

Từ trang Hóa đơn, có thể navigate đến:
- Trang chủ (Dashboard)
- Hàng hóa (Products)
- Bán hàng (POS)
- Khách hàng (Customers)
- Nhân viên (Employees)
- Báo cáo (Reports)
- Nhà sản xuất (Manufacturer)

## ⚠️ Lưu ý:

1. **Backend API**: Cần đảm bảo backend API đang chạy trên `http://localhost:5001`
2. **CORS**: Backend cần config CORS để cho phép frontend kết nối
3. **Authentication**: Cần có token trong localStorage để gọi API
4. **Database**: Cần có bảng `invoices` và `invoice_details` trong database

## 🐛 Troubleshooting:

### Lỗi "Cannot connect to API":
- Kiểm tra backend có đang chạy không
- Kiểm tra URL trong `environment.ts`
- Kiểm tra CORS config

### Lỗi 401 Unauthorized:
- Kiểm tra token trong localStorage
- Đăng nhập lại nếu token hết hạn

### Không hiển thị dữ liệu:
- Mở Console để xem response từ API
- Kiểm tra format response có đúng không
- Kiểm tra có data trong database không

## ✅ Hoàn thành!

Trang Chi tiết Hóa đơn đã sẵn sàng sử dụng! 🎉

Để test, có thể:
1. Chạy `ng serve` 
2. Truy cập `http://localhost:4200/invoices`
3. Hoặc click vào link "Hóa đơn" trong navigation bar
