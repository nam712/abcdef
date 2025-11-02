import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CustomerService, Customer } from '../services/customer.service';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NotificationBellComponent],
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.css']
})
export class CustomersComponent implements OnInit {
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

  customers: Customer[] = [];
  filteredCustomers: Customer[] = [];
  
  showDialog = false;
  isEditMode = false;
  currentCustomer: Customer = this.getEmptyCustomer();
  currentCustomerId: number | null = null;
  
  isLoading = false;
  errorMessage = '';
  showFilters = false; // Add this
  
  // Add filters object
  filters = {
    searchText: '',
    customerType: '',
    status: '',
    sortBy: 'name-asc'
  };
  
  isMobileMenuOpen = false;
  productsSubmenuOpen = false;

  constructor(
    private router: Router,
    private customerService: CustomerService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    console.log('✅ Customers component loaded!');
    const savedTheme = localStorage.getItem('dashboard-theme');
    if (savedTheme) {
      this.currentTheme = savedTheme;
      this.applyTheme(savedTheme);
    }
    this.loadCustomers();
  }

  private getEmptyCustomer(): Customer {
    return {
      customerCode: this.generateCustomerCode(),
      customerName: '',
      phone: '',
      email: '',
      address: '',
      taxCode: '',
      customerType: 'retail',
      dateOfBirth: '',
      gender: '',
      idCard: '',
      bankAccount: '',
      bankName: '',
      totalDebt: 0,
      totalPurchaseAmount: 0,
      totalPurchaseCount: 0,
      loyaltyPoints: 0,
      segment: '',
      source: '',
      avatarUrl: '',
      status: 'active',
      notes: '',
      createdAt: '',
      updatedAt: ''
    };
  }

  private generateCustomerCode(): string {
    // Generate a mini GUID (8 characters)
    return 'xxxxxxxx'.replace(/[x]/g, function(c) {
      const r = Math.random() * 16 | 0;
      return r.toString(16);
    });
  }

  loadCustomers(): void {
    this.isLoading = true;
    this.customerService.getAllCustomers().subscribe({
      next: (data) => {
        this.customers = Array.isArray(data) ? data : [];
        this.applyFilter();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error loading customers:', error);
        this.errorMessage = 'Không thể tải danh sách khách hàng';
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    let result = [...this.customers];

    // Search filter
    if (this.filters.searchText.trim()) {
      const searchLower = this.filters.searchText.toLowerCase();
      result = result.filter(c =>
        c.customerName?.toLowerCase().includes(searchLower) ||
        c.customerCode?.toLowerCase().includes(searchLower) ||
        c.phone?.includes(searchLower) ||
        c.email?.toLowerCase().includes(searchLower) ||
        c.contactPerson?.toLowerCase().includes(searchLower)
      );
    }

    // Customer type filter
    if (this.filters.customerType) {
      result = result.filter(c => c.customerType === this.filters.customerType);
    }

    // Status filter
    if (this.filters.status) {
      result = result.filter(c => c.status === this.filters.status);
    }

    // Sort
    switch (this.filters.sortBy) {
      case 'name-asc':
        result.sort((a, b) => (a.customerName || '').localeCompare(b.customerName || ''));
        break;
      case 'name-desc':
        result.sort((a, b) => (b.customerName || '').localeCompare(a.customerName || ''));
        break;
      case 'points-desc':
        result.sort((a, b) => (b.loyaltyPoints || 0) - (a.loyaltyPoints || 0));
        break;
      case 'points-asc':
        result.sort((a, b) => (a.loyaltyPoints || 0) - (b.loyaltyPoints || 0));
        break;
      case 'recent':
        result.sort((a, b) => (b.customerId || 0) - (a.customerId || 0));
        break;
    }

    this.filteredCustomers = result;
  }

  resetFilters(): void {
    this.filters = {
      searchText: '',
      customerType: '',
      status: '',
      sortBy: 'name-asc'
    };
    this.applyFilter();
    if (window.innerWidth < 1024) {
      this.showFilters = false;
      document.body.style.overflow = '';
    }
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
    document.body.style.overflow = this.showFilters ? 'hidden' : '';
  }

  hasActiveFilters(): boolean {
    return this.filters.searchText !== '' ||
           this.filters.customerType !== '' ||
           this.filters.status !== '' ||
           this.filters.sortBy !== 'name-asc';
  }

  getActiveFilterCount(): number {
    let count = 0;
    if (this.filters.searchText) count++;
    if (this.filters.customerType) count++;
    if (this.filters.status) count++;
    if (this.filters.sortBy !== 'name-asc') count++;
    return count;
  }

  openAddDialog(): void {
    this.isEditMode = false;
    this.currentCustomer = this.getEmptyCustomer();
    this.currentCustomer.customerCode = this.generateCustomerCode(); // Tạo mã mới mỗi lần mở dialog
    this.currentCustomerId = null;
    this.showDialog = true;
    this.errorMessage = '';
  }

  editCustomer(customer: Customer): void {
    this.isEditMode = true;
    this.currentCustomer = { ...customer };
    this.currentCustomerId = customer.customerId || null;
    this.showDialog = true;
    this.errorMessage = '';
  }

  deleteCustomer(customer: Customer): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa khách hàng "${customer.customerName}"?`)) return;
    
    const customerId = customer.customerId;
    if (!customerId) return;

    this.isLoading = true;
    this.customerService.deleteCustomer(customerId).subscribe({
      next: () => {
        this.notificationService.addNotification(
          `Đã xóa khách hàng "${customer.customerName}" thành công!`, 
          'success'
        );
        this.loadCustomers();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Delete error:', error);
        this.notificationService.addNotification(
          'Có lỗi xảy ra khi xóa khách hàng!', 
          'error'
        );
        this.isLoading = false;
      }
    });
  }

  saveCustomer(): void {
    this.errorMessage = '';
    if (!this.currentCustomer.customerName?.trim()) {
      this.errorMessage = 'Vui lòng nhập tên khách hàng!';
      return;
    }
    if (!this.currentCustomer.customerCode?.trim()) {
      this.errorMessage = 'Vui lòng nhập mã khách hàng!';
      return;
    }
    if (!this.currentCustomer.phone?.trim()) {
      this.errorMessage = 'Vui lòng nhập số điện thoại!';
      return;
    }
    // Kiểm tra số điện thoại đã tồn tại
    const phoneExists = this.customers.some(c => 
      c.phone === this.currentCustomer.phone && 
      c.customerId !== this.currentCustomerId
    );
    if (phoneExists) {
      this.errorMessage = 'Số điện thoại này đã được sử dụng bởi khách hàng khác!';
      return;
    }
    // Chuyển đổi đúng kiểu dữ liệu cho các trường số và ngày
    const payload: any = {
      customerCode: this.currentCustomer.customerCode || '',
      customerName: this.currentCustomer.customerName || '',
      phone: this.currentCustomer.phone || '',
      email: this.currentCustomer.email?.trim() || null, // null nếu rỗng, không gửi chuỗi rỗng
      address: this.currentCustomer.address || '',
      taxCode: this.currentCustomer.taxCode || '',
      customerType: this.currentCustomer.customerType || 'retail',
      dateOfBirth: this.currentCustomer.dateOfBirth ? new Date(this.currentCustomer.dateOfBirth).toISOString() : null,
      gender: this.currentCustomer.gender || '',
      idCard: this.currentCustomer.idCard || '',
      bankAccount: this.currentCustomer.bankAccount || '',
      bankName: this.currentCustomer.bankName || '',
      totalDebt: Number(this.currentCustomer.totalDebt) || 0,
      totalPurchaseAmount: Number(this.currentCustomer.totalPurchaseAmount) || 0,
      totalPurchaseCount: Number(this.currentCustomer.totalPurchaseCount) || 0,
      loyaltyPoints: Number(this.currentCustomer.loyaltyPoints) || 0,
      segment: this.currentCustomer.segment || '',
      source: this.currentCustomer.source || '',
      avatarUrl: this.currentCustomer.avatarUrl || '',
      status: this.currentCustomer.status || 'active',
      notes: this.currentCustomer.notes || ''
      // Không gửi customerId, createdAt, updatedAt khi thêm mới
    };

    this.isLoading = true;
    if (this.isEditMode && this.currentCustomerId) {
  payload.customerId = this.currentCustomerId; // ✅ thêm ID vào payload
  this.customerService.updateCustomer(this.currentCustomerId, payload).subscribe({
    next: () => {
      this.notificationService.addNotification(
        `Đã cập nhật khách hàng "${payload.customerName}" thành công!`, 
        'success'
      );
      this.loadCustomers();
      this.closeDialog();
      this.isLoading = false;
    },
    error: (error) => {
      console.error('❌ Update error:', error);
      this.errorMessage = 'Có lỗi xảy ra khi cập nhật';
      this.notificationService.addNotification(
        'Có lỗi xảy ra khi cập nhật khách hàng!', 
        'error'
      );
      this.isLoading = false;
    }
  });
} else {
      // Gửi đúng payload cho backend
      console.log('📤 Payload gửi đi:', payload); // Log payload
      this.customerService.createCustomer(payload).subscribe({
        next: () => {
          this.notificationService.addNotification(
            `Đã thêm khách hàng "${payload.customerName}" thành công!`, 
            'success'
          );
          this.loadCustomers();
          this.closeDialog();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Create error:', error);
          console.error('❌ Error details:', error.error); // Log chi tiết lỗi
          if (error.error && error.error.errors) {
            console.error('❌ Validation errors:', error.error.errors);
          }
          this.errorMessage = error.error?.title || 'Có lỗi xảy ra khi thêm khách hàng';
          this.notificationService.addNotification(
            'Có lỗi xảy ra khi thêm khách hàng!', 
            'error'
          );
          this.isLoading = false;
        }
      });
    }
  }

  closeDialog(): void {
    this.showDialog = false;
    this.errorMessage = '';
  }

  // Theme methods
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

  // Navigation methods
  toggleMobileMenu(): void { this.isMobileMenuOpen = !this.isMobileMenuOpen; }
  closeMobileMenu(): void { this.isMobileMenuOpen = false; }
  openProductsSubmenu(): void { this.productsSubmenuOpen = true; }
  closeProductsSubmenu(): void { this.productsSubmenuOpen = false; }
  toggleProductsSubmenu(): void { this.productsSubmenuOpen = !this.productsSubmenuOpen; }

  navigateToDashboard(): void {
    console.log('Click: Trang chủ');
    this.closeMobileMenu();
    this.router.navigate(['/dashboard']);
  }

  navigateToProducts(): void {
    console.log('Click: Hàng hóa');
    this.closeMobileMenu();
    this.closeProductsSubmenu();
    this.router.navigate(['/products']);
  }

  navigateToStockIn(): void {
    console.log('Click: Nhập kho');
    this.closeMobileMenu();
    this.closeProductsSubmenu();
    this.router.navigate(['/stock-in']);
  }

  navigateToStockOut(): void {
    console.log('Click: Xuất kho');
    this.closeMobileMenu();
    this.closeProductsSubmenu();
    this.router.navigate(['/stock-out']);
  }

  navigateToCustomers(): void {
    console.log('Click: Khách hàng');
    this.closeMobileMenu();
  }

  navigateToEmployees(): void {
    console.log('Click: Nhân viên');
    this.closeMobileMenu();
    this.router.navigate(['/employees']);
  }

  navigateToReports(): void {
    console.log('Click: Báo cáo');
    this.closeMobileMenu();
    this.router.navigate(['/reports']);
  }

  navigateToManufacturers(): void {
    console.log('Click: Nhà sản xuất');
    this.closeMobileMenu();
    this.router.navigate(['/manufacturer']);
  }

  navigateToInvoices(): void {
    console.log('Click: Hóa đơn');
    this.closeMobileMenu();
      this.router.navigate(['/invoices']);
  }

  logout(): void {
    console.log('Click: Đăng xuất');
    this.router.navigate(['/login']);
  }

  trackByCustomerId(index: number, customer: Customer): number {
    return customer.customerId || index;
  }
}
