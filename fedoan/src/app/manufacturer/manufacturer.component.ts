import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SupplierService, Supplier } from '../services/supplier.service';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-manufacturer',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NotificationBellComponent],
  templateUrl: './manufacturer.component.html',
  styleUrls: ['./manufacturer.component.css']
})
export class ManufacturerComponent implements OnInit {
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
  
  suppliers: Supplier[] = [];
  filteredSuppliers: Supplier[] = [];
  
  filters = {
    searchText: '',
    status: '',
    city: '',
    hasTaxCode: false,
    hasBankAccount: false,
    sortBy: 'name-asc'
  };

  showDialog = false;
  isEditMode = false;
  currentSupplier: Supplier = this.getEmptySupplier();
  currentSupplierId: number | null = null;
  isLoading = false;
  errorMessage = '';

  showFilters = false; // Trạng thái hiển thị filters trên mobile

  constructor(
    private router: Router,
    private supplierService: SupplierService,
    private notificationService: NotificationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    console.log('✅ Manufacturer component loaded successfully!');
    const savedTheme = localStorage.getItem('dashboard-theme');
    if (savedTheme) {
      this.currentTheme = savedTheme;
      this.applyTheme(savedTheme);
    }
    this.loadSuppliers();
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
    this.closeMobileMenu();
    this.router.navigate(['/customers']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  navigateToInvoices(): void {
    this.closeMobileMenu();
    this.router.navigate(['/invoices']);
  }

  private getEmptySupplier(): Supplier {
    return {
      supplierCode: this.generateSupplierCode(),
      supplierName: '',
      contactPerson: '',
      phone: '',
      email: '',
      address: '',
      taxCode: null,
      bankAccount: '',
      bankName: '',
      priceList: '',
      logoUrl: null,
      status: 'active',
      notes: ''
    };
  }

  private generateSupplierCode(): string {
    // Generate a mini GUID (8 characters)
    return 'xxxxxxxx'.replace(/[x]/g, function(c) {
      const r = Math.random() * 16 | 0;
      return r.toString(16);
    });
  }

  loadSuppliers(): void {
    this.isLoading = true;
    this.errorMessage = '';
    const useMockData = false;
    
    if (useMockData) {
      console.log('📦 Using mock data');
      setTimeout(() => {
        this.suppliers = [
          {
            supplierCode: 'SUP-001',
            supplierName: 'Công ty Test 1',
            contactPerson: 'Nguyễn Văn A',
            phone: '0123456789',
            email: 'test1@example.com',
            address: 'Hà Nội',
            taxCode: '0123456789',
            bankAccount: '1234567890',
            bankName: 'Vietcombank',
            priceList: 'Bảng giá 2024',
            logoUrl: null,
            status: 'active',
            notes: 'Test data'
          },
          {
            supplierCode: 'SUP-002',
            supplierName: 'Công ty Test 2',
            contactPerson: 'Trần Thị B',
            phone: '0987654321',
            email: 'test2@example.com',
            address: 'Hồ Chí Minh',
            taxCode: null,
            bankAccount: '',
            bankName: '',
            priceList: '',
            logoUrl: null,
            status: 'active',
            notes: ''
          }
        ];
        this.isLoading = false;
      }, 500);
      return;
    }
    
    console.log('🌐 Loading suppliers from API...');
    this.supplierService.getAllSuppliers().subscribe({
      next: (response) => {
        console.log('✅ Raw API Response:', JSON.stringify(response, null, 2));
        
        if (response.success) {
          let suppliersData: Supplier[] | null = null;
          const data = response.data as any; // Type cast để tránh lỗi TypeScript
          
          // Xử lý nhiều trường hợp response khác nhau
          if (Array.isArray(data)) {
            suppliersData = data;
          } else if (data && typeof data === 'object') {
            if (Array.isArray(data.items)) {
              suppliersData = data.items;
            } else if (Array.isArray(data.data)) {
              suppliersData = data.data;
            } else if (Array.isArray((data as any).suppliers)) {
              suppliersData = (data as any).suppliers;
            }
          }
          
          if (suppliersData && Array.isArray(suppliersData)) {
            this.suppliers = suppliersData;
            this.applyFilters(); // Apply filters after loading
            console.log(`✅ Loaded ${this.suppliers.length} suppliers:`, this.suppliers);
          }
        }
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error loading suppliers:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.message);
        
        this.suppliers = [];
        
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
    // Ngăn scroll khi filters mở trên mobile
    if (this.showFilters) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  hasActiveFilters(): boolean {
    return this.filters.searchText !== '' ||
           this.filters.status !== '' ||
           this.filters.city !== '' ||
           this.filters.hasTaxCode ||
           this.filters.hasBankAccount ||
           this.filters.sortBy !== 'name-asc';
  }

  getActiveFilterCount(): number {
    let count = 0;
    if (this.filters.searchText) count++;
    if (this.filters.status) count++;
    if (this.filters.city) count++;
    if (this.filters.hasTaxCode) count++;
    if (this.filters.hasBankAccount) count++;
    if (this.filters.sortBy !== 'name-asc') count++;
    return count;
  }

  resetFilters(): void {
    this.filters = {
      searchText: '',
      status: '',
      city: '',
      hasTaxCode: false,
      hasBankAccount: false,
      sortBy: 'name-asc'
    };
    this.applyFilters();
    // Đóng filters trên mobile sau khi reset
    if (window.innerWidth < 1024) {
      this.showFilters = false;
      document.body.style.overflow = '';
    }
  }

  applyFilters(): void {
    console.log('🔍 Applying filters:', this.filters);
    
    let result = [...this.suppliers];

    // Filter by search text
    if (this.filters.searchText) {
      const searchLower = this.filters.searchText.toLowerCase();
      result = result.filter(s => 
        s.supplierName?.toLowerCase().includes(searchLower) ||
        s.supplierCode?.toLowerCase().includes(searchLower) ||
        s.email?.toLowerCase().includes(searchLower) ||
        s.phone?.includes(searchLower) ||
        s.contactPerson?.toLowerCase().includes(searchLower)
      );
    }

    // Filter by status
    if (this.filters.status) {
      result = result.filter(s => s.status === this.filters.status);
    }

    // Filter by city
    if (this.filters.city) {
      result = result.filter(s => {
        if (this.filters.city === 'Khác') {
          return !['Hà Nội', 'TP.HCM', 'Đà Nẵng', 'Hải Phòng', 'Cần Thơ'].some(city => 
            s.address?.includes(city)
          );
        }
        return s.address?.includes(this.filters.city);
      });
    }

    // Filter by has tax code
    if (this.filters.hasTaxCode) {
      result = result.filter(s => s.taxCode && s.taxCode.trim() !== '');
    }

    // Filter by has bank account
    if (this.filters.hasBankAccount) {
      result = result.filter(s => s.bankAccount && s.bankAccount.trim() !== '');
    }

    // Sort
    switch (this.filters.sortBy) {
      case 'name-asc':
        result.sort((a, b) => (a.supplierName || '').localeCompare(b.supplierName || ''));
        break;
      case 'name-desc':
        result.sort((a, b) => (b.supplierName || '').localeCompare(a.supplierName || ''));
        break;
      case 'code-asc':
        result.sort((a, b) => (a.supplierCode || '').localeCompare(b.supplierCode || ''));
        break;
      case 'code-desc':
        result.sort((a, b) => (b.supplierCode || '').localeCompare(a.supplierCode || ''));
        break;
      // Add newest/oldest when you have createdDate field
    }

    this.filteredSuppliers = result;
    console.log(`✅ Filtered to ${this.filteredSuppliers.length} suppliers`);
    
    // Đóng filters trên mobile sau khi apply
    if (window.innerWidth < 1024) {
      this.showFilters = false;
      document.body.style.overflow = '';
    }
  }

  ngOnDestroy(): void {
    // Cleanup: đảm bảo body scroll được restore
    document.body.style.overflow = '';
  }

  openAddDialog(): void {
    this.isEditMode = false;
    this.currentSupplier = this.getEmptySupplier();
    this.currentSupplier.supplierCode = this.generateSupplierCode(); // Tạo mã mới mỗi lần mở dialog
    this.currentSupplierId = null;
    this.showDialog = true;
    this.errorMessage = '';
  }

  viewSupplier(supplier: any): void {
    const details = `
Mã NCC: ${supplier.supplierCode}
Tên: ${supplier.supplierName}
Người liên hệ: ${supplier.contactPerson}
Điện thoại: ${supplier.phone}
Email: ${supplier.email}
Địa chỉ: ${supplier.address}
Mã số thuế: ${supplier.taxCode || 'Chưa có'}
Tài khoản NH: ${supplier.bankAccount || 'Chưa có'}
Ngân hàng: ${supplier.bankName || 'Chưa có'}
Bảng giá: ${supplier.priceList || 'Chưa có'}
Trạng thái: ${supplier.status === 'active' ? 'Hoạt động' : 'Không hoạt động'}
Ghi chú: ${supplier.notes || 'Không có'}
    `;
    alert(details);
  }

  editSupplier(supplier: any): void {
    console.log('✏️ Editing supplier:', supplier);
    this.isEditMode = true;
    this.currentSupplier = { ...supplier };
    
    // Lấy ID từ supplier object
    this.currentSupplierId = supplier.id || supplier.supplierId || null;
    
    if (!this.currentSupplierId) {
      console.error('❌ Supplier ID not found:', supplier);
      alert('Không tìm thấy ID nhà cung cấp. Không thể chỉnh sửa.');
      return;
    }
    
    console.log('📝 Edit mode - Supplier ID:', this.currentSupplierId);
    this.showDialog = true;
    this.errorMessage = '';
  }

  deleteSupplier(supplier: any): void {
    console.log('🗑️ Attempting to delete supplier:', supplier);
    
    if (!confirm(`Bạn có chắc chắn muốn xóa nhà cung cấp "${supplier.supplierName}"?`)) {
      console.log('❌ Delete cancelled by user');
      return;
    }

    const supplierId = supplier.id || supplier.supplierId;
    
    if (!supplierId) {
      console.error('❌ Supplier ID not found:', supplier);
      alert('Không tìm thấy ID nhà cung cấp. Không thể xóa.');
      return;
    }

    console.log('🗑️ Deleting supplier ID:', supplierId);
    this.isLoading = true;
    
    this.supplierService.deleteSupplier(supplierId).subscribe({
      next: (response) => {
        console.log('✅ Delete response:', response);
        if (response.success) {
          alert('✅ Xóa nhà cung cấp thành công!');
          this.loadSuppliers(); // Reload danh sách
        } else {
          alert('❌ ' + (response.message || 'Xóa nhà cung cấp thất bại'));
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Delete error:', error);
        
        if (error.status === 404) {
          alert('❌ Không tìm thấy nhà cung cấp cần xóa');
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

  saveSupplier(): void {
    console.log('💾 Saving supplier...', this.currentSupplier);
    this.errorMessage = '';
    this.isLoading = true;

    const token = this.authService.getToken();
    console.log('🔑 Current token:', token ? 'exists' : 'not found');
    
    if (!token) {
      this.errorMessage = 'Không tìm thấy token. Vui lòng đăng nhập lại.';
      this.isLoading = false;
      setTimeout(() => this.router.navigate(['/login']), 2000);
      return;
    }

    const decoded = this.authService.decodeToken();
    console.log('🔓 Decoded token:', decoded);

    const shopOwnerId = this.authService.getShopOwnerId();
    console.log('🏪 Shop Owner ID:', shopOwnerId);
    
    if (!shopOwnerId) {
      this.errorMessage = 'Không tìm thấy thông tin shop_owner_id trong token. Vui lòng đăng nhập lại.';
      this.isLoading = false;
      setTimeout(() => this.router.navigate(['/login']), 2000);
      return;
    }

    // Clean data - remove extra fields
    const supplierData: any = {
      supplierCode: this.currentSupplier.supplierCode,
      supplierName: this.currentSupplier.supplierName,
      contactPerson: this.currentSupplier.contactPerson || '',
      phone: this.currentSupplier.phone,
      email: this.currentSupplier.email || '',
      address: this.currentSupplier.address || '',
      taxCode: this.currentSupplier.taxCode || '',
      bankAccount: this.currentSupplier.bankAccount || '',
      bankName: this.currentSupplier.bankName || '',
      priceList: this.currentSupplier.priceList || '',
      logoUrl: this.currentSupplier.logoUrl || null,
      status: this.currentSupplier.status || 'active',
      notes: this.currentSupplier.notes || '',
      shop_owner_id: parseInt(shopOwnerId, 10) // Convert to number
    };

    console.log('📦 Clean supplier data to send:', JSON.stringify(supplierData, null, 2));

    if (this.isEditMode && this.currentSupplierId) {
      // Update existing supplier
      console.log('✏️ Updating supplier ID:', this.currentSupplierId);
      this.supplierService.updateSupplier(this.currentSupplierId, supplierData).subscribe({
        next: (response) => {
          console.log('✅ Update response:', response);
          if (response.success) {
            this.notificationService.addNotification(
              `Đã cập nhật nhà cung cấp "${this.currentSupplier.supplierName}" thành công!`, 
              'success',
              {
                entityType: 'Supplier',
                entityId: this.currentSupplierId ?? undefined,
                action: 'Update',
                metadata: { supplierName: this.currentSupplier.supplierName },
                route: '/manufacturer'
              }
            );
            this.loadSuppliers();
            this.closeDialog();
          } else {
            this.errorMessage = response.message || 'Cập nhật nhà cung cấp thất bại';
            this.notificationService.addNotification(
              response.message || 'Cập nhật nhà cung cấp thất bại!', 
              'error',
              {
                entityType: 'Supplier',
                entityId: this.currentSupplierId ?? undefined,
                action: 'Update'
              }
            );
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Update error:', error);
          
          if (error.status === 401) {
            this.errorMessage = 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
            setTimeout(() => this.router.navigate(['/login']), 2000);
          } else if (error.status === 0) {
            this.errorMessage = '❌ Không thể kết nối đến API server. Vui lòng kiểm tra API có đang chạy không.';
          } else if (error.status === 404) {
            this.errorMessage = '❌ Không tìm thấy nhà cung cấp cần cập nhật';
          } else if (error.status === 400) {
            const errorMsg = error.error?.message || error.error?.errors?.join(', ') || 'Dữ liệu không hợp lệ';
            this.errorMessage = errorMsg;
          } else {
            this.errorMessage = error.error?.message || 'Có lỗi xảy ra khi cập nhật nhà cung cấp';
          }
          this.isLoading = false;
        }
      });
    } else {
      // Create new supplier
      console.log('➕ Creating new supplier');
      this.supplierService.createSupplier(supplierData).subscribe({
        next: (response) => {
          console.log('✅ Create response:', response);
          if (response.success) {
            const createdId = response.data?.supplierId;
            this.notificationService.addNotification(
              `Đã thêm nhà cung cấp "${this.currentSupplier.supplierName}" thành công!`, 
              'success',
              {
                entityType: 'Supplier',
                entityId: createdId,
                action: 'Create',
                metadata: { supplierName: this.currentSupplier.supplierName },
                route: '/manufacturer'
              }
            );
            this.loadSuppliers();
            this.closeDialog();
          } else {
            this.errorMessage = response.message || 'Thêm nhà cung cấp thất bại';
            this.notificationService.addNotification(
              response.message || 'Thêm nhà cung cấp thất bại!', 
              'error',
              {
                entityType: 'Supplier',
                action: 'Create'
              }
            );
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Create error:', error);
          console.error('❌ Error status:', error.status);
          console.error('❌ Error statusText:', error.statusText);
          console.error('❌ Error error:', error.error);
          console.error('❌ Error message:', error.message);
          
          // Log response body nếu có
          if (error.error) {
            console.error('❌ Backend error response:', JSON.stringify(error.error, null, 2));
          }
          
          if (error.status === 401) {
            this.errorMessage = 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
            setTimeout(() => this.router.navigate(['/login']), 2000);
          } else if (error.status === 0) {
            this.errorMessage = '❌ Không thể kết nối đến API server.\n\nVui lòng:\n1. Kiểm tra API server có đang chạy không\n2. Kiểm tra URL trong environment.ts\n3. Kiểm tra CORS trong API';
          } else if (error.status === 400) {
            // Xử lý chi tiết lỗi 400
            let errorMsg = 'Dữ liệu không hợp lệ';
            
            if (error.error?.message) {
              errorMsg = error.error.message;
            } else if (error.error?.errors) {
              if (Array.isArray(error.error.errors)) {
                errorMsg = error.error.errors.join(', ');
              } else if (typeof error.error.errors === 'object') {
                errorMsg = Object.values(error.error.errors).flat().join(', ');
              }
            } else if (typeof error.error === 'string') {
              errorMsg = error.error;
            }
            
            console.error('❌ 400 Bad Request - Parsed error:', errorMsg);
            
            if (errorMsg.includes('Supplier code already exists') || errorMsg.includes('already exists')) {
              this.errorMessage = '❌ Mã nhà cung cấp đã tồn tại. Đang tự động tạo mã mới...';
              setTimeout(() => {
                this.currentSupplier.supplierCode = this.generateSupplierCode();
                this.errorMessage = '';
                this.saveSupplier();
              }, 1000);
            } else if (errorMsg.toLowerCase().includes('shop_owner') || errorMsg.toLowerCase().includes('shopowner')) {
              this.errorMessage = `❌ ${errorMsg}\n\nShop Owner ID hiện tại: ${shopOwnerId}\nVui lòng đăng nhập lại hoặc liên hệ admin.`;
            } else {
              this.errorMessage = `❌ ${errorMsg}`;
            }
          } else {
            this.errorMessage = error.error?.message || error.message || 'Có lỗi xảy ra khi thêm nhà cung cấp';
          }
          this.isLoading = false;
        }
      });
    }
  }

  trackBySupplierCode(index: number, supplier: Supplier): string {
    return supplier.supplierCode;
  }
}
