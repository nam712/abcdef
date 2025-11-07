import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ProductService } from '../services/product.service';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';
import { InvoiceService } from '../services/invoice.service';
import { CustomerService, Customer } from '../services/customer.service';
import { environment } from '../../environments/environment';

interface Product {
  productId: number;
  productCode: string;
  productName: string;
  categoryId?: number;
  categoryName?: string;
  price: number;
  costPrice?: number;
  stockQuantity: number;
  unit?: string;
  barcode?: string;
  imageUrl?: string;
  description?: string;
  status?: string;
}

interface CartItem {
  product: Product;
  quantity: number;
  subtotal: number;
}

interface Notification {
  id: number;
  message: string;
  type: 'info' | 'warning' | 'success' | 'error';
  time: string;
  isRead: boolean;
}

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './pos.component.html',
  styleUrls: ['./pos.component.css']
})
export class PosComponent implements OnInit {
  currentUser = {
    name: 'Người dùng',
    email: 'user@example.com',
    avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?ixlib=rb-1.2.1&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80'
  };

  currentTheme = 'light';
  themes = [
    { name: 'light', label: 'Sáng', bgColor: '#f8fafc', primaryColor: '#6366f1' },
    { name: 'dark', label: 'Tối', bgColor: '#1e293b', primaryColor: '#8b5cf6' },
    { name: 'blue', label: 'Xanh dương', bgColor: '#eff6ff', primaryColor: '#3b82f6' },
    { name: 'green', label: 'Xanh lá', bgColor: '#f0fdf4', primaryColor: '#10b981' },
    { name: 'purple', label: 'Tím', bgColor: '#faf5ff', primaryColor: '#8b5cf6' }
  ];
  
  private previousQuantities = new Map<number, number>();

  isMobileMenuOpen = false;
  products: Product[] = [];
  filteredProducts: Product[] = [];
  searchText = '';
  isLoading = false;
  errorMessage = '';
  
  // Toast notification
  showToast = false;
  toastMessage = '';
  toastType: 'success' | 'error' | 'warning' = 'error';

  // Confirm dialog
  showConfirmDialog = false;
  confirmMessage = '';
  confirmTitle = '';
  onConfirmAction: (() => void) | null = null;

  // Notifications
  showNotifications = false;
  notifications: Notification[] = [];
  unreadNotifications = 0;
  private notificationIdCounter = 1;

  // Multi-cart management
  carts: Array<{
    id: number;
    name: string;
    items: CartItem[];
    createdAt: Date;
  }> = [];
  customers: Customer[] = [];
  filteredCustomers: Customer[] = [];
  customerSearchText = '';
  selectedCustomerId?: number | null = null;
  createdInvoices: any[] = [];
  
  // Keep discount/computation cache if needed
  
  activeCartIndex = 0;
  nextCartId = 1;

  // Getter for active cart items
  get cartItems(): CartItem[] {
    return this.carts[this.activeCartIndex]?.items || [];
  }

  set cartItems(items: CartItem[]) {
    if (this.carts[this.activeCartIndex]) {
      this.carts[this.activeCartIndex].items = items;
    }
  }

  constructor(
    private router: Router,
    private productService: ProductService,
    private globalNotificationService: NotificationService,
    private invoiceService: InvoiceService,
    private customerService: CustomerService
  ) {}

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('dashboard-theme');
    if (savedTheme) {
      this.currentTheme = savedTheme;
      this.applyTheme(savedTheme);
    }
    // Tạo giỏ hàng đầu tiên
    this.createNewCart();
    this.loadProducts();
    this.loadCustomers();
  this.loadCreatedInvoices();
    
    // Thêm thông báo chào mừng
    this.addNotification('Chào mừng đến với hệ thống bán hàng!', 'success');
  }

  private loadCreatedInvoices(): void {
    try {
      const raw = localStorage.getItem('createdInvoices');
      if (raw) this.createdInvoices = JSON.parse(raw) || [];
    } catch (e) {
      this.createdInvoices = [];
    }
  }

  private saveCreatedInvoice(inv: any): void {
    if (!inv) return;
    // Normalize minimal fields
    const item = {
      invoiceId: inv.invoiceId || inv.InvoiceId || inv.id,
      invoiceCode: inv.invoiceCode || inv.InvoiceCode || inv.code,
      finalAmount: inv.finalAmount ?? inv.FinalAmount ?? inv.totalAmount ?? 0,
      createdAt: inv.createdAt || inv.CreatedAt || new Date().toISOString(),
      raw: inv
    };
    // add to beginning
    this.createdInvoices.unshift(item);
    // keep last 20
    if (this.createdInvoices.length > 20) this.createdInvoices = this.createdInvoices.slice(0, 20);
    localStorage.setItem('createdInvoices', JSON.stringify(this.createdInvoices));
  }

  getInvoicePrintUrl(invoiceId: number | undefined): string {
    if (!invoiceId) return '#';
    return `${environment.apiUrl}/api/Invoices/${invoiceId}/print`;
  }

  clearCreatedInvoices(): void {
    this.createdInvoices = [];
    try { localStorage.removeItem('createdInvoices'); } catch { }
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getAllProducts().subscribe({
      next: (response: any) => {
        const data = Array.isArray(response) ? response : (response.data || []);
        
        console.log('📦 Raw API response:', response);
        console.log('📦 Data array length:', data.length);
        if (data.length > 0) {
          console.log('📦 Raw first product:', data[0]);
          console.log('📦 All keys:', Object.keys(data[0]));
        }
        
        // Map và xử lý URL ảnh
        this.products = data
          .filter((p: any) => p.status === 'active')
          .map((p: any) => {
            const mapped = {
              ...p,
              imageUrl: p.imageUrl || p.ImageUrl || this.getDefaultImage(),
              stockQuantity: p.stock ?? p.Stock ?? p.stockQuantity ?? p.StockQuantity ?? 0,
              price: p.price ?? p.Price ?? 0,
              productName: p.productName ?? p.ProductName ?? 'Unknown',
              productCode: p.productCode ?? p.ProductCode ?? '',
              unit: p.unit ?? p.Unit ?? 'sp'
            };
            console.log('Mapped product:', mapped.productName, 'Stock:', mapped.stockQuantity);
            return mapped;
          });
        this.filteredProducts = [...this.products];
        console.log('� Loaded products:', this.products.length);
        console.log('�️ Mapped sample product:', this.products[0]);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error loading products:', error);
        this.errorMessage = 'Không thể tải danh sách sản phẩm';
        this.isLoading = false;
      }
    });
  }
  searchProducts(): void {
    const search = this.searchText.toLowerCase().trim();
    if (!search) {
      this.filteredProducts = [...this.products];
      return;
    }
    this.filteredProducts = this.products.filter(p =>
      p.productName.toLowerCase().includes(search) ||
      p.productCode.toLowerCase().includes(search) ||
      (p.barcode && p.barcode.toLowerCase().includes(search))
    );
  }

  // Tính số lượng còn lại của sản phẩm sau khi trừ đi các đơn khác
  getAvailableStock(productId: number): number {
    const product = this.products.find(p => p.productId === productId);
    if (!product) return 0;

    // Tính tổng số lượng đã dùng trong TẤT CẢ các giỏ hàng
    let totalUsed = 0;
    this.carts.forEach(cart => {
      const item = cart.items.find(i => i.product.productId === productId);
      if (item) {
        totalUsed += item.quantity;
      }
    });

    return product.stockQuantity - totalUsed;
  }

  // Tính số lượng còn lại cho giỏ hàng hiện tại (trừ các giỏ khác)
  getAvailableStockForCurrentCart(productId: number): number {
    const product = this.products.find(p => p.productId === productId);
    if (!product) return 0;

    // Tính tổng số lượng đã dùng trong các giỏ hàng KHÁC (không bao gồm giỏ hiện tại)
    let totalUsedInOtherCarts = 0;
    this.carts.forEach((cart, index) => {
      if (index !== this.activeCartIndex) {
        const item = cart.items.find(i => i.product.productId === productId);
        if (item) {
          totalUsedInOtherCarts += item.quantity;
        }
      }
    });

    return product.stockQuantity - totalUsedInOtherCarts;
  }
  addToCart(product: Product): void {
    // Kiểm tra số lượng còn lại sau khi trừ các giỏ khác
    const availableStock = this.getAvailableStockForCurrentCart(product.productId);
    
    if (availableStock <= 0) {
      this.showNotification('Sản phẩm này đã hết hàng!', 'error');
      return;
    }

    const existingItem = this.cartItems.find((item: CartItem) => item.product.productId === product.productId);
    
    if (existingItem) {
      // Kiểm tra nếu đã đạt tối đa số lượng available
      if (existingItem.quantity >= availableStock) {
        this.showNotification('Sản phẩm này đã hết hàng!', 'error');
        return;
      }
      existingItem.quantity++;
      existingItem.subtotal = existingItem.quantity * existingItem.product.price;
      // Cập nhật giá trị trong map
      this.previousQuantities.set(product.productId, existingItem.quantity);
    } else {
      const newItem = {
        product: product,
        quantity: 1,
        subtotal: product.price
      };
      this.cartItems.push(newItem);
      // Lưu giá trị ban đầu
      this.previousQuantities.set(product.productId, 1);
    }
  }

  onQuantityInput(item: CartItem, event: Event): void {
    const input = event.target as HTMLInputElement;
    const inputValue = input.value;
    
    // Parse giá trị nhập vào
    let quantity = parseInt(inputValue) || 0;
    
    // Tính số lượng available (trừ đi các giỏ khác)
    const availableStock = this.getAvailableStockForCurrentCart(item.product.productId);
    
    // Kiểm tra và giới hạn số lượng ngay lập tức
    if (quantity > availableStock) {
      // Hiển thị thông báo
      this.showNotification('Sản phẩm này đã hết hàng!', 'error');
      
      // Tự động điều chỉnh về số lượng tối đa
      quantity = availableStock;
      item.quantity = quantity;
      input.value = quantity.toString();
      
      // Lưu giá trị mới
      this.previousQuantities.set(item.product.productId, quantity);
    } else if (quantity <= 0) {
      // Nếu nhập số âm hoặc 0, xóa khỏi giỏ hàng
      const previousQty = this.previousQuantities.get(item.product.productId) || 1;
      this.showConfirm(
        'Xóa sản phẩm',
        `Bạn có muốn xóa "${item.product.productName}" khỏi giỏ hàng?`,
        () => {
          this.removeFromCart(item);
          this.previousQuantities.delete(item.product.productId);
        }
      );
      // Khôi phục về giá trị trước đó
      quantity = previousQty;
      item.quantity = quantity;
      input.value = quantity.toString();
    } else {
      item.quantity = quantity;
      // Lưu giá trị hợp lệ
      this.previousQuantities.set(item.product.productId, quantity);
    }
    
    // Cập nhật subtotal
    item.subtotal = item.quantity * item.product.price;
  }

  validateQuantity(item: CartItem): void {
    // Khi blur, validate và fix số lượng nếu cần
    if (item.quantity <= 0) {
      this.removeFromCart(item);
      return;
    }
    const availableStock = this.getAvailableStockForCurrentCart(item.product.productId);
    if (item.quantity > availableStock) {
      // Reset về số lượng tối đa
      item.quantity = availableStock;
      item.subtotal = item.quantity * item.product.price;
    }
  }

  updateQuantity(item: CartItem, quantity: number): void {
    if (quantity <= 0) {
      this.removeFromCart(item);
      return;
    }
    const availableStock = this.getAvailableStockForCurrentCart(item.product.productId);
    if (quantity > availableStock) {
      // Hiển thị thông báo
      this.showNotification('Sản phẩm này đã hết hàng!', 'error');
      return;
    }
    item.quantity = quantity;
    item.subtotal = item.quantity * item.product.price;
    // Cập nhật giá trị trong map
    this.previousQuantities.set(item.product.productId, quantity);
  }

  removeFromCart(item: CartItem): void {
    const index = this.cartItems.indexOf(item);
    if (index > -1) {
      this.cartItems.splice(index, 1);
      // Xóa giá trị trong map
      this.previousQuantities.delete(item.product.productId);
    }
  }

  clearCart(): void {
    this.showConfirm(
      'Xóa giỏ hàng',
      'Bạn có chắc muốn xóa toàn bộ giỏ hàng?',
      () => {
        this.cartItems = [];
        this.previousQuantities.clear();
        this.showNotification('Đã xóa toàn bộ giỏ hàng', 'success');
      }
    );
  }

  getTotalItems(): number {
    return this.cartItems.reduce((sum: number, item: CartItem) => sum + item.quantity, 0);
  }

  getTotalAmount(): number {
    return this.cartItems.reduce((sum: number, item: CartItem) => sum + item.subtotal, 0);
  }

  checkout(): void {
    if (this.cartItems.length === 0) {
      this.showNotification('Giỏ hàng trống!', 'warning');
      return;
    }
    // Compute display total: after applying points discount but BEFORE promo
    const totalItems = this.getTotalItems();
    const totalBeforePromo = Math.max(0, this.getTotalAmount() - this.getPointsDiscountAmount());
    // Confirm then create invoice from current cart
    this.showConfirm(
      'Tạo hóa đơn',
      `Bạn có muốn tạo hóa đơn cho ${totalItems} sản phẩm - Tổng: ${totalBeforePromo.toLocaleString()} đ (chưa trừ khuyến mãi)?`,
      () => this.createInvoiceFromCart()
    );
  }

  loadCustomers(): void {
    this.customerService.getAllCustomers().subscribe({
      next: (response: any) => {
        // customer service returns an array
        const data = Array.isArray(response) ? response : (response.data || []);
        this.customers = data;
        this.filteredCustomers = [...this.customers];
          // Do not auto-select a customer on load; cashier should pick a customer explicitly.
      },
      error: (err) => {
        console.warn('Không thể tải khách hàng', err);
      }
    });
  }

  onCustomerSearch(): void {
    const q = (this.customerSearchText || '').toString().trim();
    if (!q) {
      this.filteredCustomers = [...this.customers];
      return;
    }

    // First try client-side filter for responsiveness
    const lc = q.toLowerCase();
    const localMatches = this.customers.filter(c => {
      return (c.customerName || '').toLowerCase().includes(lc) || (c.phone || '').toLowerCase().includes(lc) || (c.customerCode || '').toLowerCase().includes(lc);
    });

    if (localMatches.length > 0) {
      this.filteredCustomers = localMatches;
      // If the query looks like a phone number and we have an exact single-phone match, auto-select it
      if (this.isPhoneQuery()) {
        const digits = q.replace(/\D/g, '');
        const exact = localMatches.find(c => (c.phone || '').replace(/\D/g, '') === digits);
        if (exact) {
          // small timeout to allow UI to update suggestions briefly
          setTimeout(() => this.selectCustomer(exact), 50);
          return;
        }
        // if only one local match, select it
        if (localMatches.length === 1) {
          setTimeout(() => this.selectCustomer(localMatches[0]), 50);
          return;
        }
      } else {
        return;
      }
    }

    // Fallback: call backend search if no local match (useful for phone lookup)
    this.customerService.searchCustomers(q).subscribe({
      next: (res: any) => {
        const list = Array.isArray(res) ? res : (res.data || []);
        this.filteredCustomers = list;
        // if the query is phone-like and backend returned exactly one match, auto-select it
        if (this.isPhoneQuery() && Array.isArray(list) && list.length === 1) {
          setTimeout(() => this.selectCustomer(list[0]), 50);
        }
      },
      error: (err) => {
        console.warn('Lỗi khi tìm kiếm khách hàng', err);
        this.filteredCustomers = [];
      }
    });
  }

  /**
   * Return true when the current customerSearchText appears to be a phone query
   * We treat it as phone when it contains at least 6 digits.
   */
  isPhoneQuery(): boolean {
    const q = (this.customerSearchText || '').toString();
    const digits = q.replace(/\D/g, '');
    return digits.length >= 6;
  }

  selectCustomer(c: Customer): void {
    // When cashier explicitly selects a customer, load latest details from backend
    const id = c.customerId || (c as any).CustomerId;
    if (!id) return;
    this.customerService.getCustomerById(id).subscribe({
      next: (resp: any) => {
        const payload = Array.isArray(resp) ? resp[0] : (resp?.data || resp?.Data || resp);
        if (payload) {
          // update local list (replace existing entry or add to front)
          const idx = this.customers.findIndex(x => x.customerId === id);
          if (idx > -1) {
            this.customers[idx] = payload;
          } else {
            this.customers.unshift(payload);
          }
          this.selectedCustomerId = id;
          this.customerSearchText = `${payload.customerName} (${payload.phone || ''})`;
          // clear suggestions
          this.filteredCustomers = [];
        }
      },
      error: (err) => {
        // fallback to local data if API fails
        this.selectedCustomerId = id as number;
        this.customerSearchText = `${c.customerName} (${c.phone})`;
        this.filteredCustomers = [];
      }
    });
  }

  selectOrCreateCustomer(): void {
    // If user has already selected a customer ID, nothing to do
    if (this.selectedCustomerId) return;

    const input = (this.customerSearchText || '').toString().trim();
    if (!input) {
      // No input: create a generic walk-in
      this.createTemporaryCustomerAndProceed('','Khách lẻ');
      return;
    }

    // If input matches a shown customer (e.g. user typed name exactly), pick it
    const match = this.customers.find(c => {
      return (c.customerName || '').toLowerCase() === input.toLowerCase() || (c.phone || '') === input;
    });
    if (match) {
      this.selectCustomer(match);
      return;
    }

    // Otherwise create temporary customer using input as phone if it looks numeric, else as name
    const digits = input.replace(/\D/g, '');
    if (digits.length >= 6) {
      this.createTemporaryCustomerAndProceed(input, 'Khách lẻ');
    } else {
      this.createTemporaryCustomerAndProceed('', input);
    }
  }

  private createTemporaryCustomerAndProceed(phone: string, name: string): void {
    const timestamp = Date.now();
    const tempCustomer: any = {
      customerCode: `KH-LE-${timestamp}`,
      customerName: name || 'Khách lẻ',
      phone: phone || '',
      status: 'active'
    };

    this.customerService.createCustomer(tempCustomer).subscribe({
      next: (created: any) => {
        const id = created?.customerId || created?.CustomerId || created?.id;
        if (id) {
          this.customers.unshift(created);
          this.selectedCustomerId = id;
          this.customerSearchText = `${created.customerName} (${created.phone || ''})`;
          // proceed to create invoice
          this.createInvoiceFromCart();
        } else {
          this.showNotification('Không thể tạo khách hàng tạm thời', 'error');
        }
      },
      error: (err) => {
        console.error('Lỗi khi tạo khách hàng tạm thời', err);
        this.showNotification('Lỗi tạo khách hàng tạm thời', 'error');
      }
    });
  }

  private updateCustomerLoyalty(customerId: number, deductPoints: number, addPoints: number): void {
    // Fetch latest customer then update loyaltyPoints
    this.customerService.getCustomerById(customerId).subscribe({
      next: (resp: any) => {
        const cust = Array.isArray(resp) ? resp[0] : (resp?.data || resp?.Data || resp);
        if (!cust) return;
        const current = cust.loyaltyPoints ?? cust.LoyaltyPoints ?? 0;
        let nextPoints = (Number(current) || 0) - (Number(deductPoints) || 0) + (Number(addPoints) || 0);
        if (nextPoints < 0) nextPoints = 0;
        // normalize field name
        cust.loyaltyPoints = nextPoints;
        // ensure CustomerId/CustomerId present for PUT
        if (!cust.customerId && cust.CustomerId) cust.customerId = cust.CustomerId;
        const id = cust.customerId || cust.CustomerId;
        try {
          this.customerService.updateCustomer(id, cust).subscribe({
            next: () => {
              this.showNotification(`Cập nhật điểm khách hàng: ${nextPoints} điểm`, 'success');
              // update local list
              const idx = this.customers.findIndex(c => c.customerId === id);
              if (idx > -1) {
                this.customers[idx].loyaltyPoints = nextPoints;
              }
            },
            error: (err) => {
              console.warn('Lỗi khi cập nhật điểm khách hàng', err);
            }
          });
        } catch (e) {
          console.warn('Failed to update customer loyalty', e);
        }
      },
      error: (err) => {
        console.warn('Không lấy được thông tin khách hàng để cập nhật điểm', err);
      }
    });
  }

  private generateInvoiceCode(): string {
    const d = new Date();
    const pad = (n: number) => n.toString().padStart(2, '0');
    const code = `INV-${d.getFullYear()}${pad(d.getMonth()+1)}${pad(d.getDate())}${pad(d.getHours())}${pad(d.getMinutes())}${pad(d.getSeconds())}`;
    return code;
  }

  getSelectedCustomer(): Customer | undefined {
    return this.customers.find(c => c.customerId === this.selectedCustomerId);
  }

  // Loyalty policy: 1 point = 1,000 đ (configurable)
  pointsRate: number = 1000; // value in VND per point

  // Return available loyalty points for selected customer
  getPointsAvailable(): number {
    const cust = this.getSelectedCustomer();
    return Number((cust?.loyaltyPoints ?? (cust as any)?.LoyaltyPoints ?? 0)) || 0;
  }

  // Compute discount amount (VND) coming from points usage, capped to order total
  getPointsDiscountAmount(totalAmount?: number): number {
    const t = totalAmount ?? this.getTotalAmount();
    if (t <= 0) return 0;
    const available = this.getPointsAvailable();
    const possible = available * this.pointsRate;
    return Math.min(possible, t);
  }

  // Compute how many points will be used given total (floor)
  getPointsToUse(totalAmount?: number): number {
    const amount = this.getPointsDiscountAmount(totalAmount);
    return Math.floor(amount / this.pointsRate);
  }

  // Promotion (cart-level)
  promoPercent: number = 0; // percentage promo entered by cashier
  promoFixed: number = 0; // fixed amount discount entered by cashier

  getPromoAmount(afterLoyaltyTotal?: number): number {
    // Do not apply promo when no customer was selected
    if (!this.selectedCustomerId) return 0;

    const base = afterLoyaltyTotal ?? (this.getTotalAmount() - this.getPointsDiscountAmount());
    if (base <= 0) return 0;
    // Fixed promo takes precedence if set
    if (this.promoFixed && this.promoFixed > 0) {
      return Math.min(this.promoFixed, base);
    }
    if (this.promoPercent && this.promoPercent > 0) {
      return Math.round((base * this.promoPercent) / 100);
    }
    return 0;
  }

  createInvoiceFromCart(): void {
    const hadCustomerBefore = !!this.selectedCustomerId;

    const proceedWithCustomerId = (customerId: number) => {
      const details = this.cartItems.map(item => ({
        productId: item.product.productId,
        quantity: item.quantity,
        unitPrice: item.product.price
      }));

  const totalAmount = this.getTotalAmount();
  // Points-based loyalty discount (1 point = pointsRate VND)
  // Only apply loyalty discount if there was a selected customer before checkout
  const loyaltyDiscount = hadCustomerBefore ? this.getPointsDiscountAmount(totalAmount) : 0;
  const afterLoyalty = Math.max(0, totalAmount - loyaltyDiscount);
  const promoAmount = this.getPromoAmount(afterLoyalty);
  const discountAmount = loyaltyDiscount + promoAmount;
  const finalAmount = Math.max(0, totalAmount - discountAmount);

      const payload: any = {
        invoiceCode: this.generateInvoiceCode(),
        customerId: customerId,
        employeeId: null,
        invoiceDate: new Date().toISOString(),
        discountAmount: discountAmount,
        // include total after applying points but BEFORE promo so data retains that value
        totalBeforePromo: afterLoyalty,
        amountPaid: finalAmount, // mark as paid immediately
        paymentMethodId: null,
        notes: `Tạo từ POS - ${this.carts[this.activeCartIndex]?.name || ''}`,
        details: details
      };

      this.invoiceService.createInvoice(payload).subscribe({
        next: (res: any) => {
          const ok = !!(res && (res.success || res.Success));
          if (ok) {
            this.showNotification('Tạo hóa đơn thành công', 'success');
            // save created invoice for persistence across navigation
            const created = res?.data || res?.Data || res;
            // Save a normalized local record that uses the total BEFORE promo as the displayed amount
            const savedLocal = {
              invoiceId: created?.invoiceId || created?.InvoiceId || created?.id,
              invoiceCode: created?.invoiceCode || created?.InvoiceCode || payload.invoiceCode,
              finalAmount: afterLoyalty, // store amount before promo as requested
              createdAt: created?.createdAt || created?.CreatedAt || new Date().toISOString(),
              raw: created
            };
            this.saveCreatedInvoice(savedLocal);
            // Update customer loyalty points: deduct used, add earned
            // Only update loyalty if customer existed before checkout (do not grant/deduct for temporary walk-in)
            if (hadCustomerBefore && customerId) {
              const loyaltyUsedPoints = this.getPointsToUse(totalAmount); // points consumed for loyalty discount
              const earnedPoints = Math.floor(finalAmount / this.pointsRate); // earn 1 point per pointsRate VND
              this.updateCustomerLoyalty(customerId, loyaltyUsedPoints, earnedPoints);
            }
            // clear current cart
            this.cartItems = [];
            this.previousQuantities.clear();
            // Optionally navigate to invoice detail page if available
          } else {
            const message = res?.message || 'Tạo hóa đơn thất bại';
            this.showNotification(message, 'error');
            console.warn('Create invoice response', res);
          }
        },
        error: (err) => {
          console.error('Lỗi khi tạo hóa đơn', err);
          const errorMsg = err?.error?.message || 'Lỗi khi gọi API tạo hóa đơn';
          this.showNotification(errorMsg, 'error');
        }
      });
    };

    if (!this.selectedCustomerId) {
      // Create a temporary "walk-in" customer and then proceed
      const timestamp = Date.now();
      const tempCustomer: any = {
        customerCode: `KH-LE-${timestamp}`,
        customerName: 'Khách lẻ',
        phone: '',
        status: 'active'
      };

      this.customerService.createCustomer(tempCustomer).subscribe({
        next: (created: any) => {
          // created may be ApiResponse or plain object
          const payload = created?.data || created?.Data || created;
          const id = payload?.customerId || payload?.CustomerId || payload?.id;
          if (id) {
            // add to local customers list so dropdown will show it
            this.customers.unshift(payload);
            this.selectedCustomerId = id;
            proceedWithCustomerId(id as number);
          } else {
            // fallback: show warning
            this.showNotification('Không thể tạo khách hàng tạm thời', 'error');
          }
        },
        error: (err) => {
          console.error('Lỗi khi tạo khách hàng tạm thời', err);
          this.showNotification('Lỗi tạo khách hàng tạm thời', 'error');
        }
      });
    } else {
      proceedWithCustomerId(this.selectedCustomerId as number);
    }
  }

  // Multi-cart methods
  createNewCart(): void {
    const newCart = {
      id: this.nextCartId++,
      name: `Đơn ${this.nextCartId}`,
      items: [],
      createdAt: new Date()
    };
    this.carts.push(newCart);
    this.activeCartIndex = this.carts.length - 1;
    this.showNotification(`Đã tạo ${newCart.name}`, 'success');
  }

  switchCart(index: number): void {
    if (index >= 0 && index < this.carts.length) {
      this.activeCartIndex = index;
    }
  }

  deleteCart(index: number, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    
    if (this.carts.length === 1) {
      this.showNotification('Không thể xóa giỏ hàng cuối cùng!', 'warning');
      return;
    }

    const cart = this.carts[index];
    
    const performDelete = () => {
      this.carts.splice(index, 1);
      
      // Điều chỉnh activeCartIndex
      if (this.activeCartIndex >= this.carts.length) {
        this.activeCartIndex = this.carts.length - 1;
      } else if (this.activeCartIndex > index) {
        this.activeCartIndex--;
      }
      
      this.showNotification(`Đã xóa ${cart.name}`, 'success');
    };

    if (cart.items.length > 0) {
      this.showConfirm(
        'Xóa đơn hàng',
        `Xóa "${cart.name}" với ${cart.items.length} sản phẩm?`,
        performDelete
      );
    } else {
      performDelete();
    }
  }

  getCartItemCount(cartIndex: number): number {
    return this.carts[cartIndex]?.items.reduce((sum: number, item: CartItem) => sum + item.quantity, 0) || 0;
  }

  getCartTotal(cartIndex: number): number {
    return this.carts[cartIndex]?.items.reduce((sum: number, item: CartItem) => sum + item.subtotal, 0) || 0;
  }

  changeTheme(): void {
    const currentIndex = this.themes.findIndex(theme => theme.name === this.currentTheme);
    const nextIndex = (currentIndex + 1) % this.themes.length;
    const newTheme = this.themes[nextIndex];
    
    this.currentTheme = newTheme.name;
    this.applyTheme(newTheme.name);
    localStorage.setItem('dashboard-theme', newTheme.name);
  }

  private applyTheme(themeName: string): void {
    const theme = this.themes.find(t => t.name === themeName);
    if (theme) {
      document.documentElement.style.setProperty('--bg-color', theme.bgColor);
      document.documentElement.style.setProperty('--primary-color', theme.primaryColor);
      document.body.className = `theme-${themeName}`;
    }
  }

  getCurrentThemeLabel(): string {
    const theme = this.themes.find(t => t.name === this.currentTheme);
    return theme ? theme.label : 'Sáng';
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  showNotification(message: string, type: 'success' | 'error' | 'warning' = 'error'): void {
    this.toastMessage = message;
    this.toastType = type;
    this.showToast = true;
    
    // Thêm vào danh sách thông báo local
    this.addNotification(message, type);
    
    // Thêm vào global notification service
    this.globalNotificationService.addNotification(message, type);
    
    // Tự động ẩn sau 3 giây
    setTimeout(() => {
      this.showToast = false;
    }, 3000);
  }

  showConfirm(title: string, message: string, onConfirm: () => void): void {
    this.confirmTitle = title;
    this.confirmMessage = message;
    this.onConfirmAction = onConfirm;
    this.showConfirmDialog = true;
  }

  confirmYes(): void {
    if (this.onConfirmAction) {
      this.onConfirmAction();
    }
    this.closeConfirm();
  }

  confirmNo(): void {
    this.closeConfirm();
  }

  closeConfirm(): void {
    this.showConfirmDialog = false;
    this.confirmTitle = '';
    this.confirmMessage = '';
    this.onConfirmAction = null;
  }

  navigateToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  navigateToProducts(): void {
    this.router.navigate(['/products']);
  }

  navigateToCustomers(): void {
    this.router.navigate(['/customers']);
  }

  navigateToEmployees(): void {
    this.router.navigate(['/employees']);
  }

  navigateToReports(): void {
    this.router.navigate(['/reports']);
  }

  navigateToManufacturers(): void {
    this.router.navigate(['/manufacturer']);
  }

  navigateToInvoices(): void {
    this.router.navigate(['/invoices']);
  }

  navigateToPos(): void {
    this.router.navigate(['/pos']);
  }

  getDefaultImage(): string {
    // SVG placeholder as data URL to avoid external dependency
    return 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="150" height="150"%3E%3Crect width="150" height="150" fill="%23ddd"/%3E%3Ctext x="50%25" y="50%25" text-anchor="middle" dy=".3em" fill="%23999" font-family="Arial, sans-serif" font-size="14"%3ENo Image%3C/text%3E%3C/svg%3E';
  }

  getImageUrl(product: Product): string {
    if (!product.imageUrl) {
      return this.getDefaultImage();
    }
    // Nếu là URL đầy đủ (http/https)
    if (product.imageUrl.startsWith('http')) {
      return product.imageUrl;
    }
    // Nếu là relative path, thêm base URL
    return `${environment.apiUrl}/${product.imageUrl}`;
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = this.getDefaultImage();
  }

  // Notification methods
  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
  }

  closeNotifications(): void {
    this.showNotifications = false;
  }

  addNotification(message: string, type: 'info' | 'warning' | 'success' | 'error'): void {
    const notification: Notification = {
      id: this.notificationIdCounter++,
      message,
      type,
      time: this.getRelativeTime(new Date()),
      isRead: false
    };
    
    this.notifications.unshift(notification);
    this.unreadNotifications++;
    
    // Giới hạn số lượng thông báo
    if (this.notifications.length > 50) {
      this.notifications = this.notifications.slice(0, 50);
    }
  }

  markAsRead(notification: Notification): void {
    if (!notification.isRead) {
      notification.isRead = true;
      this.unreadNotifications = Math.max(0, this.unreadNotifications - 1);
    }
  }

  markAllAsRead(): void {
    this.notifications.forEach(n => n.isRead = true);
    this.unreadNotifications = 0;
  }

  private getRelativeTime(date: Date): string {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Vừa xong';
    if (diffMins < 60) return `${diffMins} phút trước`;
    if (diffHours < 24) return `${diffHours} giờ trước`;
    if (diffDays < 7) return `${diffDays} ngày trước`;
    return date.toLocaleDateString('vi-VN');
  }

  logout(): void {
    this.showConfirm(
      'Đăng xuất',
      'Bạn có chắc muốn đăng xuất?',
      () => {
        // ✅ Xóa đúng key 'auth_token' thay vì 'authToken'
        localStorage.removeItem('auth_token');
        localStorage.removeItem('access_token'); // Xóa cả access_token nếu có
        this.router.navigate(['/login']);
      }
    );
  }
}