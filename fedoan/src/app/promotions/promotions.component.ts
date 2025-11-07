import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { PromotionService, Promotion } from '../services/promotion.service';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-promotions',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NotificationBellComponent],
  templateUrl: './promotions.component.html',
  styleUrls: ['./promotions.component.css']
})
export class PromotionsComponent implements OnInit {
  promotions: Promotion[] = [];
  filteredPromotions: Promotion[] = [];
  
  showDialog = false;
  isEditMode = false;
  currentPromotion: Promotion = this.getEmptyPromotion();
  currentPromotionId: number | null = null;
  
  isLoading = false;
  errorMessage = '';
  showFilters = false;
  
  filters = {
    searchText: '',
    promotionType: '',
    status: '',
    sortBy: 'name-asc'
  };

  // Navigation methods (thêm các method giống customers)
  currentUser = {
    name: 'Người dùng',
    email: 'user@example.com',
    avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?ixlib=rb-1.2.1&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80'
  };

  currentTheme = 'light';
  isMobileMenuOpen = false;
  productsSubmenuOpen = false;

  constructor(
    private router: Router,
    private promotionService: PromotionService,
    private notificationService: NotificationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    console.log('✅ Promotions component loaded!');
    this.loadPromotions();
  }

  private getEmptyPromotion(): Promotion {
    const today = new Date().toISOString().split('T')[0];
    const nextMonth = new Date();
    nextMonth.setMonth(nextMonth.getMonth() + 1);
    
    return {
      promotionCode: this.generatePromotionCode(),
      promotionName: '',
      description: '',
      promotionType: 'percentage',
      discountValue: 0,
      minPurchaseAmount: 0,
      maxDiscountAmount: 0,
      startDate: today,
      endDate: nextMonth.toISOString().split('T')[0],
      status: 'active',
      usageLimit: 0,
      usageCount: 0,
      applicableProducts: [],
      applicableCustomers: []
    };
  }

  private generatePromotionCode(): string {
    return 'PROMO' + Date.now().toString().slice(-6);
  }

  loadPromotions(): void {
    this.isLoading = true;
    this.promotionService.getAllPromotions().subscribe({
      next: (response) => {
        if (response && response.success && response.data) {
          this.promotions = Array.isArray(response.data) ? response.data : [];
        } else if (Array.isArray(response)) {
          this.promotions = response;
        } else {
          this.promotions = [];
        }
        this.applyFilter();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error loading promotions:', error);
        this.errorMessage = 'Không thể tải danh sách khuyến mãi';
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    let result = [...this.promotions];

    if (this.filters.searchText.trim()) {
      const searchLower = this.filters.searchText.toLowerCase();
      result = result.filter(p =>
        p.promotionName?.toLowerCase().includes(searchLower) ||
        p.promotionCode?.toLowerCase().includes(searchLower) ||
        p.description?.toLowerCase().includes(searchLower)
      );
    }

    if (this.filters.promotionType) {
      result = result.filter(p => p.promotionType === this.filters.promotionType);
    }

    if (this.filters.status) {
      result = result.filter(p => p.status === this.filters.status);
    }

    switch (this.filters.sortBy) {
      case 'name-asc':
        result.sort((a, b) => (a.promotionName || '').localeCompare(b.promotionName || ''));
        break;
      case 'name-desc':
        result.sort((a, b) => (b.promotionName || '').localeCompare(a.promotionName || ''));
        break;
      case 'recent':
        result.sort((a, b) => (b.promotionId || 0) - (a.promotionId || 0));
        break;
    }

    this.filteredPromotions = result;
  }

  openAddDialog(): void {
    this.isEditMode = false;
    this.currentPromotion = this.getEmptyPromotion();
    this.currentPromotionId = null;
    this.showDialog = true;
    this.errorMessage = '';
  }

  editPromotion(promotion: Promotion): void {
    this.isEditMode = true;
    this.currentPromotion = { ...promotion };
    this.currentPromotionId = promotion.promotionId || null;
    this.showDialog = true;
    this.errorMessage = '';
  }

  deletePromotion(promotion: Promotion): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa khuyến mãi "${promotion.promotionName}"?`)) return;
    
    const promotionId = promotion.promotionId;
    if (!promotionId) return;

    this.isLoading = true;
    this.promotionService.deletePromotion(promotionId).subscribe({
      next: () => {
        this.notificationService.addNotification(
          `Đã xóa khuyến mãi "${promotion.promotionName}" thành công!`, 
          'success'
        );
        this.loadPromotions();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Delete error:', error);
        this.notificationService.addNotification('Có lỗi xảy ra khi xóa khuyến mãi!', 'error');
        this.isLoading = false;
      }
    });
  }

  savePromotion(): void {
    this.errorMessage = '';
    
    if (!this.currentPromotion.promotionName?.trim()) {
      this.errorMessage = 'Vui lòng nhập tên khuyến mãi!';
      return;
    }
    if (!this.currentPromotion.promotionCode?.trim()) {
      this.errorMessage = 'Vui lòng nhập mã khuyến mãi!';
      return;
    }

    const shopOwnerId = this.authService.getShopOwnerId();
    if (!shopOwnerId) {
      this.errorMessage = 'Không tìm thấy thông tin shop_owner_id.';
      return;
    }

    const payload: any = {
      promotionCode: this.currentPromotion.promotionCode.trim(),
      promotionName: this.currentPromotion.promotionName.trim(),
      description: this.currentPromotion.description?.trim() || '',
      promotionType: this.currentPromotion.promotionType,
      discountValue: Number(this.currentPromotion.discountValue) || 0,
      minPurchaseAmount: Number(this.currentPromotion.minPurchaseAmount) || 0,
      maxDiscountAmount: Number(this.currentPromotion.maxDiscountAmount) || 0,
      startDate: new Date(this.currentPromotion.startDate).toISOString(),
      endDate: new Date(this.currentPromotion.endDate).toISOString(),
      status: this.currentPromotion.status,
      usageLimit: Number(this.currentPromotion.usageLimit) || null,
      usageCount: Number(this.currentPromotion.usageCount) || 0,
      shopOwnerId: parseInt(shopOwnerId, 10)
    };

    this.isLoading = true;

    if (this.isEditMode && this.currentPromotionId) {
      this.promotionService.updatePromotion(this.currentPromotionId, payload).subscribe({
        next: () => {
          this.notificationService.addNotification(
            `Đã cập nhật khuyến mãi "${payload.promotionName}" thành công!`, 
            'success'
          );
          this.loadPromotions();
          this.closeDialog();
          this.isLoading = false;
        },
        error: (error) => {
          this.handleError(error);
          this.isLoading = false;
        }
      });
    } else {
      this.promotionService.createPromotion(payload).subscribe({
        next: () => {
          this.notificationService.addNotification(
            `Đã thêm khuyến mãi "${payload.promotionName}" thành công!`, 
            'success'
          );
          this.loadPromotions();
          this.closeDialog();
          this.isLoading = false;
        },
        error: (error) => {
          this.handleError(error);
          this.isLoading = false;
        }
      });
    }
  }

  private handleError(error: any): void {
    if (error.status === 401) {
      this.errorMessage = 'Phiên đăng nhập đã hết hạn.';
      setTimeout(() => this.router.navigate(['/login']), 2000);
    } else {
      this.errorMessage = error.error?.message || 'Có lỗi xảy ra';
      this.notificationService.addNotification(this.errorMessage, 'error');
    }
  }

  closeDialog(): void {
    this.showDialog = false;
    this.errorMessage = '';
  }

  resetFilters(): void {
    this.filters = {
      searchText: '',
      promotionType: '',
      status: '',
      sortBy: 'name-asc'
    };
    this.applyFilter();
  }

  getPromotionTypeLabel(type: string): string {
    const labels: any = {
      'percentage': 'Giảm %',
      'fixed': 'Giảm cố định',
      'buy_x_get_y': 'Mua X tặng Y',
      'free_shipping': 'Miễn phí ship'
    };
    return labels[type] || type;
  }

  getStatusLabel(status: string): string {
    const labels: any = {
      'active': 'Hoạt động',
      'inactive': 'Tạm ngưng',
      'expired': 'Hết hạn'
    };
    return labels[status] || status;
  }

  toggleMobileMenu(): void { 
    this.isMobileMenuOpen = !this.isMobileMenuOpen; 
  }
  
  closeMobileMenu(): void { 
    this.isMobileMenuOpen = false; 
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
    this.router.navigate(['/products']);
  }

  navigateToCustomers(): void {
    this.closeMobileMenu();
    this.router.navigate(['/customers']);
  }

  navigateToPromotions(): void {
    this.closeMobileMenu();
    // Already on promotions page
  }

  navigateToEmployees(): void {
    this.closeMobileMenu();
    this.router.navigate(['/employees']);
  }

  navigateToInvoices(): void {
    this.closeMobileMenu();
    this.router.navigate(['/invoices']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  changeTheme(): void {
    const themes = ['light', 'dark', 'blue'];
    const currentIndex = themes.indexOf(this.currentTheme);
    this.currentTheme = themes[(currentIndex + 1) % themes.length];
    localStorage.setItem('dashboard-theme', this.currentTheme);
  }

  getCurrentThemeLabel(): string {
    const labels: any = {
      'light': 'Sáng',
      'dark': 'Tối',
      'blue': 'Xanh'
    };
    return labels[this.currentTheme] || 'Sáng';
  }

  trackByPromotionId(index: number, promotion: Promotion): number {
    return promotion.promotionId || index;
  }
}
