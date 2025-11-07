import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CustomerService, Customer } from '../services/customer.service';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';

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
    private notificationService: NotificationService,
    private authService: AuthService
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
      contactPerson: ''
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
      next: (response) => {
        console.log('✅ Raw API Response:', response);
        
        if (response && response.success && response.data) {
          this.customers = Array.isArray(response.data) ? response.data : [];
        } else if (Array.isArray(response)) {
          this.customers = response;
        } else {
          this.customers = [];
        }
        
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
    
    // Validation
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

    // Check duplicate phone
    const phoneExists = this.customers.some(c => 
      c.phone === this.currentCustomer.phone && 
      c.customerId !== this.currentCustomerId
    );
    if (phoneExists) {
      this.errorMessage = 'Số điện thoại này đã được sử dụng bởi khách hàng khác!';
      return;
    }

    // Get shop_owner_id from token
    const shopOwnerId = this.authService.getShopOwnerId();
    if (!shopOwnerId) {
      this.errorMessage = 'Không tìm thấy thông tin shop_owner_id. Vui lòng đăng nhập lại.';
      this.notificationService.addNotification(
        'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.', 
        'error'
      );
      setTimeout(() => this.router.navigate(['/login']), 2000);
      return;
    }

    // Prepare payload
    const payload: any = {
      customerCode: this.currentCustomer.customerCode?.trim(),
      customerName: this.currentCustomer.customerName?.trim(),
      phone: this.currentCustomer.phone?.trim(),
      email: this.currentCustomer.email?.trim() || '',
      address: this.currentCustomer.address?.trim() || '',
      taxCode: this.currentCustomer.taxCode?.trim() || '',
      customerType: this.currentCustomer.customerType || 'retail',
      dateOfBirth: this.currentCustomer.dateOfBirth ? new Date(this.currentCustomer.dateOfBirth).toISOString() : null,
      gender: this.currentCustomer.gender || '',
      idCard: this.currentCustomer.idCard?.trim() || '',
      bankAccount: this.currentCustomer.bankAccount?.trim() || '',
      bankName: this.currentCustomer.bankName?.trim() || '',
      totalDebt: Number(this.currentCustomer.totalDebt) || 0,
      totalPurchaseAmount: Number(this.currentCustomer.totalPurchaseAmount) || 0,
      totalPurchaseCount: Number(this.currentCustomer.totalPurchaseCount) || 0,
      loyaltyPoints: Number(this.currentCustomer.loyaltyPoints) || 0,
      segment: this.currentCustomer.segment || '',
      source: this.currentCustomer.source || '',
      avatarUrl: this.currentCustomer.avatarUrl || '',
      status: this.currentCustomer.status || 'active',
      notes: this.currentCustomer.notes?.trim() || '',
      shop_owner_id: parseInt(shopOwnerId, 10)
    };

    console.log('📤 Payload gửi đi:', payload);

    this.isLoading = true;

    if (this.isEditMode && this.currentCustomerId) {
      // Update
      payload.customerId = this.currentCustomerId;
      
      this.customerService.updateCustomer(this.currentCustomerId, payload).subscribe({
        next: (response) => {
          console.log('✅ Update response:', response);
          this.notificationService.addNotification(
            `Đã cập nhật khách hàng "${payload.customerName}" thành công!`, 
            'success',
            {
              entityType: 'Customer',
              entityId: this.currentCustomerId!,
              action: 'Update',
              route: '/customers'
            }
          );
          this.loadCustomers();
          this.closeDialog();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Update error:', error);
          this.handleError(error);
          this.isLoading = false;
        }
      });
    } else {
      // Create
      this.customerService.createCustomer(payload).subscribe({
        next: (response: any) => {
          console.log('✅ Create response:', response);
          
          const customerId = response?.data?.customerId || response?.customerId || null;
          
          this.notificationService.addNotification(
            `Đã thêm khách hàng "${payload.customerName}" thành công!`, 
            'success',
            {
              entityType: 'Customer',
              entityId: customerId,
              action: 'Create',
              route: '/customers'
            }
          );
          this.loadCustomers();
          this.closeDialog();
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

  private handleError(error: any): void {
    console.error('❌ Error details:', {
      status: error.status,
      message: error.message,
      error: error.error
    });
    
    if (error.status === 401) {
      this.errorMessage = 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
      this.notificationService.addNotification(
        'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.', 
        'error'
      );
      setTimeout(() => this.router.navigate(['/login']), 2000);
    } else if (error.status === 0) {
      this.errorMessage = '❌ Không thể kết nối đến API server.';
      this.notificationService.addNotification('Không thể kết nối đến server!', 'error');
    } else if (error.status === 400) {
      const errorMsg = error.error?.message || error.error?.errors?.join(', ') || 'Dữ liệu không hợp lệ';
      this.errorMessage = `❌ ${errorMsg}`;
      this.notificationService.addNotification(errorMsg, 'error');
    } else if (error.status === 500) {
      // Backend error - log chi tiết để debug
      console.error('❌ 500 Internal Server Error');
      console.error('❌ Error response:', error.error);
      
      let errorDetail = '';
      if (error.error) {
        if (typeof error.error === 'string') {
          errorDetail = error.error;
        } else if (error.error.message) {
          errorDetail = error.error.message;
        } else if (error.error.title) {
          errorDetail = error.error.title;
        }
      }
      
      this.errorMessage = `❌ Lỗi server (500): ${errorDetail || 'Vui lòng kiểm tra backend logs'}`;
      this.notificationService.addNotification(
        'Lỗi server! Vui lòng kiểm tra backend console.', 
        'error'
      );
      
      // Log thêm payload đã gửi để debug
      console.error('❌ Payload đã gửi:', this.currentCustomer);
    } else {
      this.errorMessage = error.error?.message || error.message || 'Có lỗi xảy ra';
      this.notificationService.addNotification(this.errorMessage, 'error');
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
    // Already on customers page
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

  navigateToPromotions(): void {
    console.log('Click: Khuyến mãi');
    this.closeMobileMenu();
    this.router.navigate(['/promotions']);
  }

  logout(): void {
    console.log('Click: Đăng xuất');
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  trackByCustomerId(index: number, customer: Customer): number {
    return customer.customerId || index;
  }
}
