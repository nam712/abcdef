import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InvoiceService } from '../services/invoice.service';
import { Invoice } from '../models/invoice.model';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';
import { CustomerService } from '../services/customer.service';
import { EmployeeService } from '../services/employee.service';
import { PromotionService } from '../services/promotion.service';

@Component({
  selector: 'app-invoice',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NotificationBellComponent],
  templateUrl: './invoice.component.html',
  styleUrls: ['./invoice.component.css']
})
export class InvoiceComponent implements OnInit, OnDestroy {
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

  isMobileMenuOpen = false;
  productsSubmenuOpen = false;
  
  invoices: Invoice[] = [];
  filteredInvoices: Invoice[] = [];
  
  // Thêm các property còn thiếu
  invoiceItems: any[] = [];
  selectedCustomerId: number | null = null;
  appliedPromotion: any = null;
  promotionCode: string = '';
  isValidatingPromotion = false;

  filters = {
    searchText: '',
    paymentStatus: '',
    dateFrom: '',
    dateTo: '',
    minAmount: null as number | null,
    maxAmount: null as number | null,
    sortBy: 'date-desc'
  };

  showDialog = false;
  isEditMode = false;
  currentInvoice: Invoice = this.getEmptyInvoice();
  currentInvoiceId: number | null = null;
  isLoading = false;
  errorMessage = '';

  showFilters = false;

  // Lists used in create/edit dialogs
  customers: any[] = [];
  employees: any[] = [];
  isLoadingCustomers = false;
  isLoadingEmployees = false;
  customersError = '';
  employeesError = '';

  constructor(
    private router: Router,
    private invoiceService: InvoiceService,
    private notificationService: NotificationService,
    private customerService: CustomerService,
    private employeeService: EmployeeService,
    private promotionService: PromotionService
  ) {}

  ngOnInit(): void {
    console.log('✅ Invoice component loaded successfully!');
    const savedTheme = localStorage.getItem('dashboard-theme');
    if (savedTheme) {
      this.currentTheme = savedTheme;
      this.applyTheme(savedTheme);
    }
    this.loadInvoices();
  }

  ngOnDestroy(): void {
    document.body.style.overflow = '';
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

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  openProductsSubmenu(): void {
    this.productsSubmenuOpen = true;
  }

  closeProductsSubmenu(): void {
    this.productsSubmenuOpen = false;
  }

  toggleProductsSubmenu(): void {
    this.productsSubmenuOpen = !this.productsSubmenuOpen;
  }

  navigateToDashboard(): void {
    this.closeMobileMenu();
    this.router.navigate(['/dashboard']);
  }

  navigateToProducts(): void {
    this.closeMobileMenu();
    this.closeProductsSubmenu();
    this.router.navigate(['/products']);
  }
  
  navigateToPurchaseOrders(): void {
    this.closeMobileMenu();
    this.router.navigate(['/invoices']);
  }

  navigateToStockIn(): void {
    this.closeMobileMenu();
    this.closeProductsSubmenu();
    this.router.navigate(['/stock-in']);
  }

  navigateToStockOut(): void {
    this.closeMobileMenu();
    this.closeProductsSubmenu();
    this.router.navigate(['/stock-out']);
  }

  navigateToEmployees(): void {
    this.closeMobileMenu();
    this.router.navigate(['/employees']);
  }

  navigateToReports(): void {
    this.closeMobileMenu();
    this.router.navigate(['/reports']);
  }

  navigateToCustomers(): void {
    this.router.navigate(['/customers']);
  }

  navigateToManufacturer(): void {
    this.closeMobileMenu();
    this.router.navigate(['/manufacturer']);
  }

  navigateToInvoices(): void {
    this.closeMobileMenu();
    this.router.navigate(['/invoices']);
  }

  logout(): void {
    this.router.navigate(['/login']);
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  private getEmptyInvoice(): Invoice {
    return {
      invoiceCode: this.generateInvoiceCode(),
      customerId: 0,
      employeeId: null,
      invoiceDate: new Date().toISOString().split('T')[0],
      totalAmount: 0,
      discountAmount: 0,
      finalAmount: 0,
      amountPaid: 0,
      paymentMethodId: null,
      paymentStatus: 'unpaid',
      notes: ''
    };
  }

  private generateInvoiceCode(): string {
    const date = new Date();
    const year = date.getFullYear().toString().substr(-2);
    const month = ('0' + (date.getMonth() + 1)).slice(-2);
    const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
    return `INV${year}${month}${random}`;
  }

  loadInvoices(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    console.log('🌐 Loading invoices from API...');
    this.invoiceService.getAllInvoices().subscribe({
      next: (response) => {
        console.log('✅ Raw API Response:', JSON.stringify(response, null, 2));
        
        if (response.success) {
          let invoicesData: Invoice[] | null = null;
          const data = response.data as any;
          
          if (Array.isArray(data)) {
            invoicesData = data;
          } else if (data && typeof data === 'object') {
            if (Array.isArray(data.items)) {
              invoicesData = data.items;
            } else if (Array.isArray(data.data)) {
              invoicesData = data.data;
            } else if (Array.isArray((data as any).invoices)) {
              invoicesData = (data as any).invoices;
            }
          }
          
          if (invoicesData && Array.isArray(invoicesData)) {
            this.invoices = invoicesData;
            this.applyFilters();
            console.log(`✅ Loaded ${this.invoices.length} invoices:`, this.invoices);
          }
        }
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error loading invoices:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.message);
        
        this.invoices = [];
        
        if (error.status === 0) {
          this.errorMessage = '❌ Không thể kết nối đến API server.\n\nĐảm bảo:\n1. API đang chạy trên http://localhost:5001\n2. Không có lỗi CORS\n3. Firewall không block';
        } else if (error.status === 404) {
          this.errorMessage = '❌ API endpoint không tồn tại. Kiểm tra URL: ' + error.url;
        } else {
          this.errorMessage = `❌ Lỗi ${error.status}: ${error.message || 'Không xác định'}`;
        }
        
        this.isLoading = false;
      }
    });
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
    if (this.showFilters) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  hasActiveFilters(): boolean {
    return this.filters.searchText !== '' ||
           this.filters.paymentStatus !== '' ||
           this.filters.dateFrom !== '' ||
           this.filters.dateTo !== '' ||
           this.filters.minAmount !== null ||
           this.filters.maxAmount !== null ||
           this.filters.sortBy !== 'date-desc';
  }

  getActiveFilterCount(): number {
    let count = 0;
    if (this.filters.searchText) count++;
    if (this.filters.paymentStatus) count++;
    if (this.filters.dateFrom) count++;
    if (this.filters.dateTo) count++;
    if (this.filters.minAmount !== null) count++;
    if (this.filters.maxAmount !== null) count++;
    if (this.filters.sortBy !== 'date-desc') count++;
    return count;
  }

  resetFilters(): void {
    this.filters = {
      searchText: '',
      paymentStatus: '',
      dateFrom: '',
      dateTo: '',
      minAmount: null,
      maxAmount: null,
      sortBy: 'date-desc'
    };
    this.applyFilters();
    if (window.innerWidth < 1024) {
      this.showFilters = false;
      document.body.style.overflow = '';
    }
  }

  applyFilters(): void {
    console.log('🔍 Applying filters:', this.filters);
    
    let result = [...this.invoices];

    // Filter by search text
    if (this.filters.searchText) {
      const searchLower = this.filters.searchText.toLowerCase();
      result = result.filter(i => 
        i.invoiceCode?.toLowerCase().includes(searchLower) ||
        i.customerName?.toLowerCase().includes(searchLower) ||
        i.employeeName?.toLowerCase().includes(searchLower) ||
        i.notes?.toLowerCase().includes(searchLower)
      );
    }

    // Filter by payment status
    if (this.filters.paymentStatus) {
      result = result.filter(i => i.paymentStatus === this.filters.paymentStatus);
    }

    // Filter by date from
    if (this.filters.dateFrom) {
      result = result.filter(i => {
        const invoiceDate = new Date(i.invoiceDate);
        const filterDate = new Date(this.filters.dateFrom);
        return invoiceDate >= filterDate;
      });
    }

    // Filter by date to
    if (this.filters.dateTo) {
      result = result.filter(i => {
        const invoiceDate = new Date(i.invoiceDate);
        const filterDate = new Date(this.filters.dateTo);
        return invoiceDate <= filterDate;
      });
    }

    // Filter by min amount
    if (this.filters.minAmount !== null) {
      result = result.filter(i => i.finalAmount >= (this.filters.minAmount || 0));
    }

    // Filter by max amount
    if (this.filters.maxAmount !== null) {
      result = result.filter(i => i.finalAmount <= (this.filters.maxAmount || Number.MAX_VALUE));
    }

    // Sort
    switch (this.filters.sortBy) {
      case 'date-desc':
        result.sort((a, b) => new Date(b.invoiceDate).getTime() - new Date(a.invoiceDate).getTime());
        break;
      case 'date-asc':
        result.sort((a, b) => new Date(a.invoiceDate).getTime() - new Date(b.invoiceDate).getTime());
        break;
      case 'amount-desc':
        result.sort((a, b) => b.finalAmount - a.finalAmount);
        break;
      case 'amount-asc':
        result.sort((a, b) => a.finalAmount - b.finalAmount);
        break;
      case 'code-asc':
        result.sort((a, b) => (a.invoiceCode || '').localeCompare(b.invoiceCode || ''));
        break;
      case 'code-desc':
        result.sort((a, b) => (b.invoiceCode || '').localeCompare(a.invoiceCode || ''));
        break;
    }

    this.filteredInvoices = result;
    console.log(`✅ Filtered to ${this.filteredInvoices.length} invoices`);
    
    if (window.innerWidth < 1024) {
      this.showFilters = false;
      document.body.style.overflow = '';
    }
  }

  openAddDialog(): void {
    this.isEditMode = false;
    this.currentInvoice = this.getEmptyInvoice();
    this.currentInvoiceId = null;
    // Load lists for dropdowns before showing dialog
    this.loadCustomersAndEmployees();
    this.showDialog = true;
    this.errorMessage = '';
  }

  private loadCustomersAndEmployees(): void {
    // Customers
    this.isLoadingCustomers = true;
    this.customersError = '';
    this.customerService.getAllCustomers().subscribe({
      next: (res) => {
        this.customers = Array.isArray(res) ? res : [];
        this.isLoadingCustomers = false;
      },
      error: (err) => {
        console.error('❌ Error loading customers:', err);
        this.customers = [];
        this.customersError = err?.message || 'Không thể tải danh sách khách hàng';
        this.isLoadingCustomers = false;
      }
    });

    // Employees
    this.isLoadingEmployees = true;
    this.employeesError = '';
    this.employeeService.getAllEmployees().subscribe({
      next: (res) => {
        this.employees = Array.isArray(res) ? res : [];
        this.isLoadingEmployees = false;
      },
      error: (err) => {
        console.error('❌ Error loading employees:', err);
        this.employees = [];
        this.employeesError = err?.message || 'Không thể tải danh sách nhân viên';
        this.isLoadingEmployees = false;
      }
    });
  }

  viewInvoice(invoice: Invoice): void {
    const details = `
Mã hóa đơn: ${invoice.invoiceCode}
Khách hàng: ${invoice.customerName || 'N/A'}
Nhân viên: ${invoice.employeeName || 'N/A'}
Ngày tạo: ${new Date(invoice.invoiceDate).toLocaleDateString('vi-VN')}
Tổng tiền: ${this.formatCurrency(invoice.totalAmount)}
Giảm giá: ${this.formatCurrency(invoice.discountAmount || 0)}
Thành tiền: ${this.formatCurrency(invoice.finalAmount)}
Đã thanh toán: ${this.formatCurrency(invoice.amountPaid || 0)}
Trạng thái: ${this.getPaymentStatusLabel(invoice.paymentStatus)}
Phương thức TT: ${invoice.paymentMethodName || 'Chưa có'}
Ghi chú: ${invoice.notes || 'Không có'}
    `;
    alert(details);
  }

  editInvoice(invoice: Invoice): void {
    console.log('✏️ Editing invoice:', invoice);
    this.isEditMode = true;
    this.currentInvoice = { ...invoice };
    
    this.currentInvoiceId = invoice.invoiceId || null;
    
    if (!this.currentInvoiceId) {
      console.error('❌ Invoice ID not found:', invoice);
      alert('Không tìm thấy ID hóa đơn. Không thể chỉnh sửa.');
      return;
    }
    
    console.log('📝 Edit mode - Invoice ID:', this.currentInvoiceId);
    this.showDialog = true;
    this.errorMessage = '';
  }

  deleteInvoice(invoice: Invoice): void {
    console.log('🗑️ Attempting to delete invoice:', invoice);
    
    if (!confirm(`Bạn có chắc chắn muốn xóa hóa đơn "${invoice.invoiceCode}"?`)) {
      console.log('❌ Delete cancelled by user');
      return;
    }

    const invoiceId = invoice.invoiceId;
    
    if (!invoiceId) {
      console.error('❌ Invoice ID not found:', invoice);
      alert('Không tìm thấy ID hóa đơn. Không thể xóa.');
      return;
    }

    console.log('🗑️ Deleting invoice ID:', invoiceId);
    this.isLoading = true;
    
    this.invoiceService.deleteInvoice(invoiceId).subscribe({
      next: (response) => {
        console.log('✅ Delete response:', response);
        if (response.success) {
          alert('✅ Xóa hóa đơn thành công!');
          this.loadInvoices();
        } else {
          alert('❌ ' + (response.message || 'Xóa hóa đơn thất bại'));
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Delete error:', error);
        
        if (error.status === 404) {
          alert('❌ Không tìm thấy hóa đơn cần xóa');
        } else if (error.status === 0) {
          alert('❌ Không thể kết nối đến server. Vui lòng kiểm tra API có đang chạy không.');
        } else {
          alert('❌ Có lỗi xảy ra khi xóa: ' + (error.error?.message || error.message));
        }
        
        this.isLoading = false;
      }
    });
  }

  closeDialog(): void {
    this.showDialog = false;
  }

  get subtotal(): number {
    return this.invoiceItems.reduce((sum: number, item: any) => sum + (item.quantity * item.unitPrice), 0);
  }

  get promotionDiscount(): number {
    return this.appliedPromotion ? this.appliedPromotion.discount : 0;
  }

  get totalAmount(): number {
    return this.subtotal - this.promotionDiscount;
  }

  applyPromotionCode(): void {
    if (!this.promotionCode.trim()) {
      this.notificationService.addNotification('Vui lòng nhập mã khuyến mãi', 'error');
      return;
    }

    this.isValidatingPromotion = true;
    this.promotionService.validatePromotion(
      this.promotionCode,
      this.subtotal,
      this.selectedCustomerId || undefined
    ).subscribe({
      next: (response) => {
        if (response.success) {
          this.appliedPromotion = response.data;
          this.notificationService.addNotification(
            `Áp dụng mã "${this.promotionCode}" thành công! Giảm ${this.formatCurrency(response.data.discount)}`,
            'success'
          );
        }
        this.isValidatingPromotion = false;
      },
      error: (error) => {
        console.error('❌ Validation error:', error);
        const message = error.error?.message || 'Mã khuyến mãi không hợp lệ';
        this.notificationService.addNotification(message, 'error');
        this.isValidatingPromotion = false;
      }
    });
  }

  removePromotion(): void {
    this.appliedPromotion = null;
    this.promotionCode = '';
    this.notificationService.addNotification('Đã hủy mã khuyến mãi', 'info');
  }

  saveInvoice(): void {
    console.log('💾 Saving invoice...', this.currentInvoice);
    this.errorMessage = '';
    this.isLoading = true;

    // Validate there is at least one product (invoice detail)
    if (!this.validateInvoiceHasProducts()) {
      this.isLoading = false;
      return;
    }

    if (this.isEditMode && this.currentInvoiceId) {
      console.log('✏️ Updating invoice ID:', this.currentInvoiceId);
      this.invoiceService.updateInvoice(this.currentInvoiceId, this.currentInvoice).subscribe({
        next: (response) => {
          console.log('✅ Update response:', response);
          if (response.success) {
            this.notificationService.addNotification(
              `Đã cập nhật hóa đơn "${this.currentInvoice.invoiceCode}" thành công!`, 
              'success',
              {
                entityType: 'Invoice',
                entityId: this.currentInvoiceId ?? undefined,
                action: 'Update',
                metadata: { invoiceCode: this.currentInvoice.invoiceCode },
                route: '/invoices'
              }
            );
            this.loadInvoices();
            this.closeDialog();
          } else {
            this.errorMessage = response.message || 'Cập nhật hóa đơn thất bại';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Update error:', error);
          this.handleError(error);
          this.isLoading = false;
        }
      });
    } else {
      console.log('➕ Creating new invoice via API');
      this.invoiceService.createInvoice(this.currentInvoice).subscribe({
        next: (response) => {
          console.log('✅ Create response:', response);
          if (response.success) {
            const createdId = response.data?.invoiceId;
            this.notificationService.addNotification(
              `Đã thêm hóa đơn "${this.currentInvoice.invoiceCode}" thành công!`, 
              'success',
              {
                entityType: 'Invoice',
                entityId: createdId,
                action: 'Create',
                metadata: { invoiceCode: this.currentInvoice.invoiceCode },
                route: '/invoices'
              }
            );
            this.loadInvoices();
            this.closeDialog();
          } else {
            this.errorMessage = response.message || 'Thêm hóa đơn thất bại';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Create error:', error);
          this.handleError(error);
          this.isLoading = false;
        }
      });
    }
  }

  private validateInvoiceHasProducts(): boolean {
    const details = this.currentInvoice.invoiceDetails;
    if (!details || !Array.isArray(details) || details.length === 0) {
      this.errorMessage = 'Hóa đơn phải có ít nhất một sản phẩm.';
      return false;
    }

    // Also ensure each detail has a valid product and positive quantity
    for (const d of details) {
      if (!d || !d.productId || d.productId <= 0) {
        this.errorMessage = 'Vui lòng chọn sản phẩm hợp lệ cho mỗi dòng.';
        return false;
      }
      if (!d.quantity || d.quantity <= 0) {
        this.errorMessage = 'Số lượng phải lớn hơn 0 cho mỗi sản phẩm.';
        return false;
      }
    }

    return true;
  }

  private handleError(error: any): void {
    if (error.status === 401) {
      this.errorMessage = 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
      setTimeout(() => this.router.navigate(['/login']), 2000);
    } else if (error.status === 0) {
      this.errorMessage = '❌ Không thể kết nối đến API server.';
    } else if (error.status === 404) {
      this.errorMessage = '❌ Không tìm thấy hóa đơn';
    } else if (error.status === 400) {
      // Normalize various shapes of validation error responses coming from the API
      const errBody = error.error;
      let errorMsg = 'Dữ liệu không hợp lệ';

      if (!errBody) {
        errorMsg = 'Dữ liệu không hợp lệ';
      } else if (typeof errBody === 'string') {
        errorMsg = errBody;
      } else if (typeof errBody === 'object') {
        if (errBody.message && typeof errBody.message === 'string') {
          errorMsg = errBody.message;
        } else if (Array.isArray(errBody)) {
          errorMsg = errBody.join(', ');
        } else if (Array.isArray((errBody as any).errors)) {
          errorMsg = (errBody as any).errors.join(', ');
        } else if (errBody.errors && typeof errBody.errors === 'object') {
          // ModelState style: { errors: { field1: ["err"], field2: ["err2"] } }
          const vals = Object.values((errBody as any).errors).flat().filter(Boolean);
          if (vals.length) errorMsg = vals.join(', ');
        } else {
          // Try to extract any string values from the object
          const vals = Object.values(errBody).filter(v => typeof v === 'string');
          if (vals.length) errorMsg = vals.join(', ');
          else {
            try {
              errorMsg = JSON.stringify(errBody);
            } catch {
              errorMsg = 'Dữ liệu không hợp lệ';
            }
          }
        }
      }

      this.errorMessage = errorMsg;
    } else {
      this.errorMessage = error.error?.message || 'Có lỗi xảy ra';
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString('vi-VN');
  }

  getPaymentStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      'paid': 'Đã thanh toán',
      'unpaid': 'Chưa thanh toán',
      'partial': 'Thanh toán 1 phần',
      'refunded': 'Đã hoàn tiền'
    };
    return labels[status] || status;
  }

  getPaymentStatusClass(status: string): string {
    const classes: { [key: string]: string } = {
      'paid': 'paid',
      'unpaid': 'unpaid',
      'partial': 'partial',
      'refunded': 'refunded'
    };
    return classes[status] || '';
  }

  trackByInvoiceId(index: number, invoice: Invoice): number {
    return invoice.invoiceId || index;
  }
}
