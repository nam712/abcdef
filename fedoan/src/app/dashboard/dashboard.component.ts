import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { ConfirmationDialogComponent } from '../shared/confirmation-dialog/confirmation-dialog.component';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ConfirmationDialogComponent, NotificationBellComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
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

  stats = [
    { title: 'Dự án đang thực hiện', value: '12', icon: 'fa-project-diagram', color: 'blue' },
    { title: 'Task hoàn thành', value: '234', icon: 'fa-check-circle', color: 'green' },
    { title: 'Thành viên team', value: '8', icon: 'fa-users', color: 'purple' },
    { title: 'Deadline sắp tới', value: '5', icon: 'fa-clock', color: 'orange' }
  ];

  recentProjects = [
    { name: 'Website Redesign', progress: 75, deadline: '2024-02-15', status: 'active' },
    { name: 'Mobile App Development', progress: 45, deadline: '2024-03-01', status: 'active' },
    { name: 'Marketing Campaign', progress: 90, deadline: '2024-01-30', status: 'active' },
    { name: 'Database Migration', progress: 30, deadline: '2024-02-28', status: 'pending' }
  ];

  upcomingTasks = [
    { title: 'Review UI Design', project: 'Website Redesign', priority: 'high', dueDate: 'Today' },
    { title: 'Test Mobile Features', project: 'Mobile App', priority: 'medium', dueDate: 'Tomorrow' },
    { title: 'Prepare Presentation', project: 'Marketing Campaign', priority: 'high', dueDate: '2 days' },
    { title: 'Update Documentation', project: 'Database Migration', priority: 'low', dueDate: '1 week' }
  ];

  isMobileMenuOpen = false;
  productsSubmenuOpen = false; // <-- new: submenu state
  showLogoutConfirm = false;

  constructor(
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    // Load saved theme
    const savedTheme = localStorage.getItem('dashboard-theme');
    if (savedTheme) {
      this.currentTheme = savedTheme;
      this.applyTheme(savedTheme);
    }
    
    // Thông báo chào mừng
    this.notificationService.addNotification('Chào mừng đến với Dashboard!', 'success');
  }

  changeTheme(): void {
    const currentIndex = this.themes.findIndex(theme => theme.name === this.currentTheme);
    const nextIndex = (currentIndex + 1) % this.themes.length;
    const newTheme = this.themes[nextIndex];
    
    this.currentTheme = newTheme.name;
    this.applyTheme(newTheme.name);
    
    // Save theme preference
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

  logout(): void {
    this.showLogoutConfirm = true;
  }

  confirmLogout(): void {
    console.log('🚪 Logging out...');
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  cancelLogout(): void {
    this.showLogoutConfirm = false;
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  // Products submenu handlers
  openProductsSubmenu(): void {
    this.productsSubmenuOpen = true;
  }

  closeProductsSubmenu(): void {
    this.productsSubmenuOpen = false;
  }

  toggleProductsSubmenu(): void {
    this.productsSubmenuOpen = !this.productsSubmenuOpen;
  }

  // Navigation methods for submenu items
  navigateToProducts(): void {
    console.log('🔍 Navigating to products...');
    this.productsSubmenuOpen = false;
    this.isMobileMenuOpen = false;
    
    this.router.navigate(['/products'])
      .then((success) => {
        console.log('✅ Navigation to products:', success);
      })
      .catch((error) => {
        console.error('❌ Navigation failed:', error);
        alert('Không thể chuyển trang. Vui lòng kiểm tra console.');
      });
  }

  navigateToStockIn(): void {
    this.productsSubmenuOpen = false;
    this.isMobileMenuOpen = false;
    console.log('Navigate to stock in (Nhập kho)');
    this.router.navigate(['/stock-in']);
  }

  navigateToStockOut(): void {
    this.productsSubmenuOpen = false;
    this.isMobileMenuOpen = false;
    console.log('Navigate to stock out (Xuất kho)');
    this.router.navigate(['/stock-out']);
  }

  navigateToTasks(): void {
    console.log('Navigate to tasks');
    this.closeMobileMenu();
  }

  navigateToCustomers(): void {
    console.log('Navigating to customers...');
    this.router.navigate(['/customers']);
    this.closeMobileMenu();
  }

  navigateToEmployees(): void {
    console.log('Navigating to employees...');
    this.router.navigate(['/employees']);
    this.closeMobileMenu();
  }

  navigateToReports(): void {
    console.log('Navigating to reports...');
    this.router.navigate(['/reports']);
    this.closeMobileMenu();
  }

  navigateToManufacturers(): void {
    console.log('🔍 Starting navigation to manufacturers...');
    this.closeMobileMenu();
    this.closeProductsSubmenu();
    
    this.router.navigate(['/manufacturer'])
      .then((result) => {
        if (result) {
          console.log('✅ Navigation to manufacturer successful');
        } else {
          console.log('❌ Navigation to manufacturer failed');
        }
      })
      .catch((error) => {
        console.error('❌ Navigation error:', error);
      });
  }

  navigateToInvoices(): void {
    console.log('🔍 Starting navigation to invoices...');
    this.closeMobileMenu();
    this.closeProductsSubmenu();
    this.router.navigate(['/invoices']);
  }
}
