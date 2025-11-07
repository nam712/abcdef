import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeService } from '../services/employee.service';
import { Router, RouterModule } from '@angular/router';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';

export interface Employee {
  employeeId?: number;
  employeeCode?: string;
  employeeName?: string;
  phone?: string;
  email?: string;
  address?: string;
  dateOfBirth?: string;
  gender?: string;
  idCard?: string;
  position?: string;
  department?: string;
  hireDate?: string;
  salary?: number;
  salaryType?: string;
  bankAccount?: string;
  bankName?: string;
  username?: string;
  password?: string;
  permissions?: string;
  avatarUrl?: string;
  workStatus?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
  status?: string;
  shop_owner_id?: number;
}

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NotificationBellComponent],
  templateUrl: './employees.component.html',
  styleUrls: ['./employees.component.css']
})
export class EmployeesComponent implements OnInit {
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

  employees: Employee[] = [];
  filteredEmployees: Employee[] = [];
  isLoading = false;
  errorMessage = '';
  showDialog = false;
  isEditMode = false;
  showFilters = false;
  isMobileMenuOpen = false;

  filters = {
    searchText: '',
    position: '',
    department: '', // Thêm dòng này
    workStatus: '', // Đổi từ 'status' sang 'workStatus'
    sortBy: 'name-asc'
  };
  currentEmployee: Employee = this.getEmptyEmployee();
  currentEmployeeId: number | null = null;

  constructor(
    private employeeService: EmployeeService, 
    private router: Router,
    private notificationService: NotificationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('dashboard-theme');
    if (savedTheme) {
      this.currentTheme = savedTheme;
      this.applyTheme(savedTheme);
    }
    this.loadEmployees();
  }

  getEmptyEmployee(): Employee {
    return {
      employeeId: 0,
      employeeCode: this.generateEmployeeCode(),
      employeeName: '',
      phone: '',
      email: '',
      address: '',
      dateOfBirth: '',
      gender: '',
      idCard: '',
      position: '',
      department: '',
      hireDate: '',
      salary: 0,
      salaryType: '',
      bankAccount: '',
      bankName: '',
      username: '',
      password: '',
      permissions: '',
      avatarUrl: '',
      workStatus: '',
      notes: '',
      createdAt: '',
      updatedAt: ''
    };
  }

  generateEmployeeCode(): string {
    // Generate a mini GUID (8 characters)
    return 'xxxxxxxx'.replace(/[x]/g, function(c) {
      const r = Math.random() * 16 | 0;
      return r.toString(16);
    });
  }

  loadEmployees(): void {
    this.isLoading = true;
    console.log('🔗 Đang gọi API:', this.employeeService['apiUrl']);
    this.employeeService.getAllEmployees().subscribe({
      next: (response) => {
        console.log('✅ Raw API Response:', response);
        
        if (response && response.success && response.data) {
          this.employees = Array.isArray(response.data) ? response.data : [];
        } else if (Array.isArray(response)) {
          this.employees = response;
        } else {
          this.employees = [];
        }
        
        this.applyFilter();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Lỗi API:', error);
        this.errorMessage = 'Không thể tải danh sách nhân viên';
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    let result = [...this.employees];
    if (this.filters.searchText.trim()) {
      const searchLower = this.filters.searchText.toLowerCase();
      result = result.filter(e =>
        (e.employeeName || '').toLowerCase().includes(searchLower) ||
        (e.employeeCode || '').toLowerCase().includes(searchLower) ||
        (e.phone || '').includes(searchLower) ||
        (e.email || '').toLowerCase().includes(searchLower) ||
        (e.position || '').toLowerCase().includes(searchLower) ||
        (e.department || '').toLowerCase().includes(searchLower)
      );
    }
    if (this.filters.position) {
      result = result.filter(e => (e.position || '').toLowerCase().includes(this.filters.position.toLowerCase()));
    }
    if (this.filters.department) {
      result = result.filter(e => (e.department || '').toLowerCase().includes(this.filters.department.toLowerCase()));
    }
    if (this.filters.workStatus) {
      result = result.filter(e => (e.workStatus || '') === this.filters.workStatus);
    }
    switch (this.filters.sortBy) {
      case 'name-asc':
        result.sort((a, b) => (a.employeeName || '').localeCompare(b.employeeName || ''));
        break;
      case 'name-desc':
        result.sort((a, b) => (b.employeeName || '').localeCompare(a.employeeName || ''));
        break;
      case 'recent':
        result.sort((a, b) => (b.employeeId || 0) - (a.employeeId || 0));
        break;
    }
    this.filteredEmployees = result;
  }

  resetFilters(): void {
    this.filters = {
      searchText: '',
      position: '',
      department: '', // Thêm dòng này
      workStatus: '',
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
           this.filters.position !== '' ||
           this.filters.workStatus !== '' ||
           this.filters.sortBy !== 'name-asc';
  }

  getActiveFilterCount(): number {
    let count = 0;
    if (this.filters.searchText) count++;
    if (this.filters.position) count++;
    if (this.filters.workStatus) count++;
    if (this.filters.sortBy !== 'name-asc') count++;
    return count;
  }

  openAddDialog(): void {
    this.isEditMode = false;
    this.currentEmployee = this.getEmptyEmployee();
    this.currentEmployee.employeeCode = this.generateEmployeeCode(); // Tạo mã mới mỗi lần mở dialog
    this.currentEmployeeId = null;
    this.showDialog = true;
    this.errorMessage = '';
  }

  editEmployee(employee: Employee): void {
    this.isEditMode = true;
    this.currentEmployee = { ...employee };
    this.currentEmployeeId = employee.employeeId || null;
    this.showDialog = true;
    this.errorMessage = '';
  }

  deleteEmployee(employee: Employee): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa nhân viên "${employee.employeeName}"?`)) return;
    const employeeId = employee.employeeId;
    if (!employeeId) return;
    this.isLoading = true;
    this.employeeService.deleteEmployee(employeeId).subscribe({
      next: () => {
        alert('✅ Xóa nhân viên thành công!');
        this.loadEmployees();
        this.isLoading = false;
      },
      error: (error) => {
        alert('Có lỗi xảy ra khi xóa nhân viên');
        this.isLoading = false;
      }
    });
  }

  saveEmployee(): void {
    this.errorMessage = '';
    
    // Validation
    if (!this.currentEmployee.employeeName?.trim()) {
      this.errorMessage = 'Vui lòng nhập tên nhân viên!';
      return;
    }
    if (!this.currentEmployee.employeeCode?.trim()) {
      this.errorMessage = 'Vui lòng nhập mã nhân viên!';
      return;
    }

    // ❌ XÓA PHẦN NÀY - không cần check shop_owner_id ở frontend
    // const shopOwnerId = this.authService.getShopOwnerId();
    // if (!shopOwnerId) { ... }

    // Prepare payload
    const payload: any = {
      employeeCode: this.currentEmployee.employeeCode?.trim() || '',
      employeeName: this.currentEmployee.employeeName?.trim() || '',
      phone: this.currentEmployee.phone?.trim() || '',
      email: this.currentEmployee.email?.trim() || '',
      address: this.currentEmployee.address?.trim() || '',
      dateOfBirth: this.currentEmployee.dateOfBirth?.trim() ? this.currentEmployee.dateOfBirth : null,
      gender: this.currentEmployee.gender || '',
      idCard: this.currentEmployee.idCard?.trim() || '',
      position: this.currentEmployee.position?.trim() || '',
      department: this.currentEmployee.department?.trim() || '',
      hireDate: this.currentEmployee.hireDate?.trim() ? this.currentEmployee.hireDate : null,
      salary: Number(this.currentEmployee.salary) || 0,
      salaryType: this.currentEmployee.salaryType || '',
      bankAccount: this.currentEmployee.bankAccount?.trim() || '',
      bankName: this.currentEmployee.bankName?.trim() || '',
      username: this.currentEmployee.username?.trim() || '',
      password: this.currentEmployee.password?.trim() || '',
      permissions: this.currentEmployee.permissions || '',
      avatarUrl: this.currentEmployee.avatarUrl || '',
      workStatus: this.currentEmployee.workStatus || 'active',
      notes: this.currentEmployee.notes?.trim() || ''
    };

    // Add employeeId for update
    if (this.isEditMode && this.currentEmployeeId) {
      payload.employeeId = this.currentEmployeeId;
    }

    console.log('📤 Payload gửi đi:', JSON.stringify(payload, null, 2));

    this.isLoading = true;

    if (this.isEditMode && this.currentEmployeeId) {
      // Update
      // If logged in user is an Employee, use the dedicated update-profile endpoint
      const userType = this.authService.getUserType();
      if (userType === 'Employee') {
        this.employeeService.updateProfile(this.currentEmployeeId, payload).subscribe({
          next: (response) => {
            console.log('✅ Update-profile response:', response);
            this.notificationService.addNotification(
              `Đã cập nhật thông tin cá nhân thành công!`,
              'success',
              {
                entityType: 'Employee',
                entityId: this.currentEmployeeId!,
                action: 'UpdateProfile',
                route: '/employees'
              }
            );
            this.loadEmployees();
            this.closeDialog();
            this.isLoading = false;
          },
          error: (error) => {
            console.error('❌ Update-profile error:', error);
            this.handleError(error);
            this.isLoading = false;
          }
        });
      } else {
        // ShopOwner or other roles use the regular update endpoint
        this.employeeService.updateEmployee(this.currentEmployeeId, payload).subscribe({
          next: (response) => {
            console.log('✅ Update response:', response);
            this.notificationService.addNotification(
              `Đã cập nhật nhân viên "${payload.employeeName}" thành công!`, 
              'success',
              {
                entityType: 'Employee',
                entityId: this.currentEmployeeId!,
                action: 'Update',
                route: '/employees'
              }
            );
            this.loadEmployees();
            this.closeDialog();
            this.isLoading = false;
          },
          error: (error) => {
            console.error('❌ Update error:', error);
            this.handleError(error);
            this.isLoading = false;
          }
        });
      }
    } else {
      // Create
      this.employeeService.createEmployee(payload).subscribe({
        next: (response: any) => {
          console.log('✅ Create response:', response);
          
          const employeeId = response?.data?.employeeId || response?.employeeId || null;
          
          this.notificationService.addNotification(
            `Đã thêm nhân viên "${payload.employeeName}" thành công!`, 
            'success',
            {
              entityType: 'Employee',
              entityId: employeeId,
              action: 'Create',
              route: '/employees'
            }
          );
          this.loadEmployees();
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
    
    console.error('❌ FULL ERROR OBJECT:', JSON.stringify(error.error, null, 2));
    
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
      let errorMsg = 'Dữ liệu không hợp lệ';
      
      if (error.error) {
        if (error.error.message) {
          errorMsg = error.error.message;
        } else if (error.error.title) {
          errorMsg = error.error.title;
        } else if (error.error.errors) {
          const errors = error.error.errors;
          const errorMessages: string[] = [];
          
          for (const field in errors) {
            if (errors.hasOwnProperty(field)) {
              const fieldErrors = errors[field];
              if (Array.isArray(fieldErrors)) {
                errorMessages.push(`${field}: ${fieldErrors.join(', ')}`);
              } else if (typeof fieldErrors === 'string') {
                errorMessages.push(`${field}: ${fieldErrors}`);
              }
            }
          }
          
          if (errorMessages.length > 0) {
            errorMsg = errorMessages.join('; ');
          }
        }
      }
      
      this.errorMessage = `❌ ${errorMsg}`;
      this.notificationService.addNotification(errorMsg, 'error');
    } else if (error.status === 500) {
      let errorDetail = 'Database error - Check backend logs';
      
      if (error.error) {
        if (typeof error.error === 'string') {
          errorDetail = error.error;
        } else if (error.error.detail) {
          errorDetail = error.error.detail;
        } else if (error.error.message) {
          errorDetail = error.error.message;
        }
      }
      
      if (errorDetail.includes('entity changes')) {
        this.errorMessage = '❌ Lỗi database:\n• Backend phải extract shop_owner_id từ JWT token\n• Kiểm tra username/email có trùng không\n• Check backend console logs';
      } else {
        this.errorMessage = `❌ Lỗi server (500): ${errorDetail}`;
      }
      
      this.notificationService.addNotification(
        'Lỗi database! Kiểm tra backend console.', 
        'error'
      );
    } else {
      this.errorMessage = error.error?.message || error.message || 'Có lỗi xảy ra';
      this.notificationService.addNotification(this.errorMessage, 'error');
    }
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

  applyTheme(themeName: string): void {
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

  closeDialog(): void {
    this.showDialog = false;
    this.errorMessage = '';
  }

  // Navigation methods
  toggleMobileMenu(): void { 
    this.isMobileMenuOpen = !this.isMobileMenuOpen; 
  }
  
  closeMobileMenu(): void { 
    this.isMobileMenuOpen = false; 
  }

  navigateToDashboard(): void { 
    this.closeMobileMenu(); 
    this.router.navigate(['/dashboard']); 
  }

  navigateToProducts(): void { 
    this.closeMobileMenu(); 
    this.router.navigate(['/products']); 
  }

  navigateToCustomers(): void { 
    this.closeMobileMenu(); 
    this.router.navigate(['/customers']); 
  }

  navigateToEmployees(): void { 
    this.closeMobileMenu(); 
    // Already on employees page
  }

  navigateToReports(): void { 
    this.closeMobileMenu(); 
    this.router.navigate(['/reports']); 
  }

  navigateToManufacturers(): void { 
    this.closeMobileMenu(); 
    this.router.navigate(['/manufacturer']); 
  }

  navigateToInvoices(): void { 
    this.closeMobileMenu(); 
    this.router.navigate(['/invoices']); 
  }

  navigateToPromotions(): void {
    console.log('Click: Khuyến mãi');
    this.closeMobileMenu();
    this.router.navigate(['/promotions']);
  }

  logout(): void { 
    this.authService.logout();
    this.router.navigate(['/login']); 
  }

  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  trackByEmployeeId(index: number, employee: Employee): number {
    return employee.employeeId || index;
  }
}
