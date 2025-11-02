import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeService } from '../services/employee.service';
import { Router, RouterModule } from '@angular/router';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';

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
    private notificationService: NotificationService
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
      next: (data) => {
        console.log('✅ Nhận dữ liệu:', data); // Log dữ liệu nhận về
        this.employees = Array.isArray(data) ? data : [];
        this.applyFilter();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Lỗi API:', error); // Log lỗi chi tiết
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
    if (!this.currentEmployee.employeeName?.trim()) {
      this.errorMessage = 'Vui lòng nhập tên nhân viên!';
      return;
    }
    if (!this.currentEmployee.employeeCode?.trim()) {
      this.errorMessage = 'Vui lòng nhập mã nhân viên!';
      return;
    }
    // Chỉ gửi các trường cần thiết cho backend khi tạo mới
    const payload: any = {
      employeeCode: this.currentEmployee.employeeCode,
      employeeName: this.currentEmployee.employeeName,
      phone: this.currentEmployee.phone,
      email: this.currentEmployee.email,
      address: this.currentEmployee.address,
      dateOfBirth: this.currentEmployee.dateOfBirth ? new Date(this.currentEmployee.dateOfBirth).toISOString() : null,
      gender: this.currentEmployee.gender,
      idCard: this.currentEmployee.idCard,
      position: this.currentEmployee.position,
      department: this.currentEmployee.department,
      hireDate: this.currentEmployee.hireDate ? new Date(this.currentEmployee.hireDate).toISOString() : null,
      salary: Number(this.currentEmployee.salary) || 0,
      salaryType: this.currentEmployee.salaryType,
      bankAccount: this.currentEmployee.bankAccount,
      bankName: this.currentEmployee.bankName,
      username: this.currentEmployee.username,
      password: this.currentEmployee.password,
      permissions: this.currentEmployee.permissions,
      avatarUrl: this.currentEmployee.avatarUrl,
      workStatus: this.currentEmployee.workStatus,
      notes: this.currentEmployee.notes
      // Không gửi employeeId, createdAt, updatedAt, status khi tạo mới
    };
    this.isLoading = true;
    if (this.isEditMode && this.currentEmployeeId) {
      payload.employeeId = this.currentEmployeeId; // Chỉ gửi khi cập nhật
      this.employeeService.updateEmployee(this.currentEmployeeId, payload).subscribe({
        next: () => {
          this.notificationService.addNotification(
            `Đã cập nhật nhân viên "${payload.employeeName}" thành công!`, 
            'success'
          );
          this.loadEmployees();
          this.closeDialog();
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = 'Có lỗi xảy ra khi cập nhật';
          this.notificationService.addNotification(
            'Có lỗi xảy ra khi cập nhật nhân viên!', 
            'error'
          );
          this.isLoading = false;
        }
      });
    } else {
      // Không gửi employeeId, createdAt, updatedAt khi tạo mới
      this.employeeService.createEmployee(payload).subscribe({
        next: () => {
          this.notificationService.addNotification(
            `Đã thêm nhân viên "${payload.employeeName}" thành công!`, 
            'success'
          );
          this.loadEmployees();
          this.closeDialog();
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = 'Có lỗi xảy ra khi thêm nhân viên';
          this.notificationService.addNotification(
            'Có lỗi xảy ra khi thêm nhân viên!', 
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

  // Navigation methods (inject Router nếu muốn chuyển trang)
  toggleMobileMenu(): void { this.isMobileMenuOpen = !this.isMobileMenuOpen; }
  closeMobileMenu(): void { this.isMobileMenuOpen = false; }
  navigateToDashboard(): void { this.closeMobileMenu(); this.router.navigate(['/dashboard']); }
  navigateToProducts(): void { this.closeMobileMenu(); this.router.navigate(['/products']); }
  navigateToCustomers(): void { this.closeMobileMenu(); this.router.navigate(['/customers']); }
  navigateToEmployees(): void { this.closeMobileMenu(); this.router.navigate(['/employees']); }
  navigateToReports(): void { this.closeMobileMenu(); this.router.navigate(['/reports']); }
  navigateToManufacturers(): void { this.closeMobileMenu(); this.router.navigate(['/manufacturer']); }
  navigateToInvoices(): void { this.closeMobileMenu(); this.router.navigate(['/invoices']); }
  logout(): void { this.router.navigate(['/login']); }

  trackByEmployeeId(index: number, employee: Employee): number {
    return employee.employeeId || index;
  }
}
