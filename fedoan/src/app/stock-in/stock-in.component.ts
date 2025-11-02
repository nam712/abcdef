import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../services/notification.service';
import { PurchaseOrderService } from '../services/purchase-order.service';
import { SupplierService } from '../services/supplier.service';
import { ProductService } from '../services/product.service';

@Component({
  selector: 'app-stock-in',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './stock-in.component.html',
  styleUrls: ['./stock-in.component.css']
})
export class StockInComponent implements OnInit {
  currentUser = { name: 'Người dùng', email: 'user@example.com', avatar: '' };
  isMobileMenuOpen = false;
  productsSubmenuOpen = false;

  stockIns: any[] = [];
  filtered: any[] = [];
  isLoading = false;
  errorMessage = '';
  showFilters = false;
  showDialog = false;
  filters = {
    searchText: '',
    supplier: '',
    status: '',
    dateFrom: '',
    dateTo: '',
    sortBy: 'date-desc'
  };

  suppliers: any[] = [];
  products: any[] = [];

  // Dialog model
  newPo: {
    poCode?: string;
    supplierId?: number | null;
    poDate?: string;
    expectedDeliveryDate?: string;
    notes?: string;
    details: Array<{ productId?: number | null; quantity: number; importPrice: number }>;
  } = { poCode: '', supplierId: null, poDate: '', expectedDeliveryDate: '', notes: '', details: [] };

  constructor(
    private router: Router,
    private notificationService: NotificationService,
    private poService: PurchaseOrderService,
    private supplierService: SupplierService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    // Load suppliers first so we can show supplier names when loading purchase orders
    this.loadSuppliersForDialog();
  }

  private loadSuppliersForDialog(): void {
    this.supplierService.getAllSuppliers(1, 200).subscribe({
      next: (response: any) => {
        console.log('📡 loadSuppliersForDialog - raw response:', response);

        // Accept array or ApiResponse shapes
        let suppliersData: any = null;
        if (Array.isArray(response)) {
          suppliersData = response;
        } else if (response && typeof response === 'object') {
          if (response.success) {
            const data = response.data;
            if (Array.isArray(data)) suppliersData = data;
            else if (data && typeof data === 'object') {
              if (Array.isArray(data.items)) suppliersData = data.items;
              else if (Array.isArray(data.data)) suppliersData = data.data;
              else if (Array.isArray((data as any).suppliers)) suppliersData = (data as any).suppliers;
            }
          } else if (Array.isArray(response.data)) {
            suppliersData = response.data;
          } else if (Array.isArray(response.items)) {
            suppliersData = response.items;
          } else if (Array.isArray((response as any).suppliers)) {
            suppliersData = (response as any).suppliers;
          }
        }

        if (suppliersData && Array.isArray(suppliersData)) {
          this.suppliers = suppliersData;
          console.log('✅ Loaded suppliers for dialog:', this.suppliers.length);
        } else {
          // fallback: empty
          this.suppliers = [];
          console.log('⚠️ Suppliers response not in expected shape, defaulting to empty');
        }

        // Always load purchase orders after attempting to load suppliers (so mapping can use suppliers if available)
        this.loadStockIns();
      },
      error: (err) => {
        console.error('❌ Error loading suppliers for dialog:', err);
        this.suppliers = [];
        this.loadStockIns();
      }
    });
  }

  navigateToStockIn(): void {
    this.closeMobileMenu();
    this.router.navigate(['/stock-in']);
  }

  navigateToProducts(): void {
    this.closeMobileMenu();
    this.closeProductsSubmenu();
    this.router.navigate(['/products']);
  }

  navigateToDashboard(): void {
    this.closeMobileMenu();
    this.router.navigate(['/dashboard']);
  }

  // Mobile/menu helpers
  toggleMobileMenu(): void { this.isMobileMenuOpen = !this.isMobileMenuOpen; }
  openProductsSubmenu(): void { this.productsSubmenuOpen = true; }

  navigateToStockOut(): void {
    this.closeMobileMenu();
    this.router.navigate(['/stock-out']);
  }

  navigateToCustomers(): void {
    this.closeMobileMenu();
    this.router.navigate(['/customers']);
  }

  navigateToEmployees(): void {
    this.closeMobileMenu();
    this.router.navigate(['/employees']);
  }

  navigateToReports(): void {
    this.closeMobileMenu();
    this.router.navigate(['/reports']);
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

  // Mobile/menu helpers (copied pattern from other components)
  toggleProductsSubmenu(): void { this.productsSubmenuOpen = !this.productsSubmenuOpen; }
  closeMobileMenu(): void { this.isMobileMenuOpen = false; }
  closeProductsSubmenu(): void { this.productsSubmenuOpen = false; }

  // Load products for dialog
  private loadProductsForDialog(): void {
    this.productService.getAllProducts().subscribe({
      next: (res: any) => {
        console.log('📡 loadProductsForDialog - raw response:', res);
        let items: any = null;
        if (Array.isArray(res)) items = res;
        else if (res && res.success) items = res.data;
        else if (res && Array.isArray(res.data)) items = res.data;
        else if (res && Array.isArray((res as any).items)) items = (res as any).items;

        if (items && typeof items === 'object' && !Array.isArray(items)) {
          if (Array.isArray((items as any).items)) items = (items as any).items;
          else if (Array.isArray((items as any).data)) items = (items as any).data;
        }

        this.products = Array.isArray(items) ? items : [];
      },
      error: (err) => {
        console.error('❌ Error loading products for dialog:', err);
        this.products = [];
      }
    });
  }

  loadStockIns(): void {
    this.isLoading = true;
    this.poService.getAll().subscribe({
      next: (res: any) => {
        console.log('📡 loadStockIns - raw response:', res);

        // Accept multiple shapes: raw array, ApiResponse { success, data }, or object with items/data
        let items: any = null;
        if (Array.isArray(res)) {
          items = res;
        } else if (res && typeof res === 'object') {
          if (res.success) {
            items = res.data;
          } else if (Array.isArray(res.data)) {
            items = res.data;
          } else if (Array.isArray(res.items)) {
            items = res.items;
          } else if (Array.isArray((res as any).purchaseOrders)) {
            items = (res as any).purchaseOrders;
          } else if ((res as any).purchaseOrderId) {
            items = [res];
          } else {
            items = res;
          }
        }

        // normalize nested containers
        if (items && typeof items === 'object' && !Array.isArray(items)) {
          if (Array.isArray((items as any).items)) items = (items as any).items;
          else if (Array.isArray((items as any).data)) items = (items as any).data;
        }

        this.stockIns = (items || []).map((po: any) => ({
          id: po.purchaseOrderId || po.id,
          code: po.poCode || po.orderCode || po.purchaseOrderCode || `PO-${po.purchaseOrderId || po.id}`,
          date: po.poDate || po.orderDate || po.purchaseDate || po.createdAt || po.purchaseOrderDate || new Date().toISOString().split('T')[0],
          // normalize supplier to a string name when possible
          supplier: po.supplierName || po.supplier?.supplierName || (typeof po.supplier === 'string' ? po.supplier : (po.supplier?.name || 'N/A')),
          supplierId: po.supplierId || po.supplier?.supplierId || po.supplier?.id || null,
          total: po.totalAmount || po.finalAmount || po.total || 0,
          status: po.status || 'Hoàn tất'
        }));
        console.log('✅ Mapped stockIns count:', this.stockIns.length, 'items:', this.stockIns.slice(0,5));

        // If supplier names are missing, try to resolve from loaded suppliers by id
        if (this.suppliers && this.suppliers.length > 0) {
          this.stockIns = this.stockIns.map(si => {
            if ((!si.supplier || si.supplier === 'N/A') && si.supplierId) {
              const s = this.suppliers.find(su => (su.id || su.supplierId) === si.supplierId || (su.id || su.supplierId) === Number(si.supplierId));
              if (s) si.supplier = s.supplierName || s.name || si.supplier;
            }
            return si;
          });
        }
        this.filtered = [...this.stockIns];
  console.log('✅ filtered set count:', this.filtered.length, 'stockIns:', this.stockIns.length);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('❌ Error loading purchase orders:', err);
        this.isLoading = false;
        this.stockIns = [];
        this.filtered = [];
      }
    });
  }

  openAddDialog(): void {
    this.resetNewPo();
    // preload suppliers & products for the dialog
    this.loadSuppliersForDialog();
    this.loadProductsForDialog();
    this.showDialog = true;
  }

  closeDialog(): void {
    this.showDialog = false;
  }

  resetNewPo(): void {
    this.newPo = { poCode: '', supplierId: null, poDate: '', expectedDeliveryDate: '', notes: '', details: [] };
  }

  addItemToNewPo(): void {
    this.newPo.details.push({ productId: null, quantity: 1, importPrice: 0 });
  }

  removeItemFromNewPo(index: number): void {
    this.newPo.details.splice(index, 1);
  }

  createPurchaseOrder(): void {
    // basic validation
    if (!this.newPo.supplierId) {
      this.notificationService.addNotification('Vui lòng chọn nhà cung cấp', 'error');
      return;
    }
    if (!this.newPo.details || this.newPo.details.length === 0) {
      this.notificationService.addNotification('Vui lòng thêm ít nhất một mặt hàng', 'error');
      return;
    }

    // Validate items

    for (const it of this.newPo.details) {
      if (!it.productId) {
        this.notificationService.addNotification('Vui lòng chọn sản phẩm cho tất cả các dòng', 'error');
        return;
      }
      if (!it.quantity || it.quantity <= 0) {
        this.notificationService.addNotification('Số lượng phải lớn hơn 0', 'error');
        return;
      }
    }

    const poDateIso = this.newPo.poDate ? new Date(this.newPo.poDate).toISOString() : new Date().toISOString();
    const expectedIso = this.newPo.expectedDeliveryDate ? new Date(this.newPo.expectedDeliveryDate).toISOString() : new Date().toISOString();

    const dto = {
      poCode: this.newPo.poCode && this.newPo.poCode.trim() ? this.newPo.poCode.trim() : `PO-${Date.now()}`,
      supplierId: Number(this.newPo.supplierId),
      poDate: poDateIso,
      expectedDeliveryDate: expectedIso,
      notes: this.newPo.notes,
      details: this.newPo.details.map(i => ({ productId: Number(i.productId), quantity: i.quantity, importPrice: i.importPrice }))
    } as any;

    console.log('Create PO DTO:', dto);

    this.poService.create(dto).subscribe({
      next: (res) => {
        // Handle both ApiResponse<{...}> and raw created object returned by backend
        let created: any = null;
        try {
          if (res && res.success) {
            created = res.data ?? res;
          } else if (res && ((res as any).purchaseOrderId || (res as any).purchaseOrderId === 0)) {
            created = res;
          } else if (res && res.data && (((res as any).data && (res as any).data.purchaseOrderId) || ((res as any).data && (res as any).data.purchaseOrderId === 0))) {
            created = res.data;
          }
        } catch (e) {
          console.warn('Error parsing create response', e, res);
        }

        if (created) {
          console.log('Create PO success - created:', created);
          // Normalize created object to the same shape as stockIns entries
          const normalized = {
            id: created.purchaseOrderId || created.purchaseOrderId,
            code: created.poCode || created.purchaseOrderCode || `PO-${created.purchaseOrderId || Date.now()}`,
            date: created.poDate || created.poDate || new Date().toISOString(),
            supplier: created.supplierName || (this.suppliers.find(s => (s.id || s.supplierId) === created.supplierId)?.supplierName || this.suppliers.find(s => (s.id || s.supplierId) === created.supplierId)?.name) || `NCC #${created.supplierId}`,
            supplierId: created.supplierId || null,
            total: created.totalAmount || created.finalAmount || created.total || 0,
            status: created.status || created.paymentStatus || 'pending'
          };

          // Insert into current lists so UI updates immediately
          this.stockIns.unshift(normalized);
          this.filtered.unshift(normalized);

          this.notificationService.addNotification('Tạo phiếu nhập thành công', 'success');
          this.closeDialog();
          // no need to reload from server
        } else {
          const msg = res?.message || 'Lỗi khi tạo phiếu nhập';
          console.warn('Create PO responded with failure:', res);
          this.notificationService.addNotification(msg, 'error');
        }
      },
      error: (err) => {
        console.error('Create PO error', err);
        console.error('Create PO error - body:', err.error);
        // Try to extract ModelState validation errors returned by ASP.NET
        let serverMsg = err.error?.msg || err.error?.message || err.message || 'Lỗi server';
        if (err.error && err.error.errors && typeof err.error.errors === 'object') {
          const errors = err.error.errors as any;
          const parts: string[] = [];
          for (const key of Object.keys(errors)) {
            const arr = errors[key];
            if (Array.isArray(arr)) parts.push(`${key}: ${arr.join(', ')}`);
            else parts.push(`${key}: ${String(arr)}`);
          }
          if (parts.length) serverMsg = parts.join(' | ');
        }

        // show notification with extracted server message(s)
        this.notificationService.addNotification('Lỗi khi tạo phiếu nhập: ' + serverMsg, 'error');
        // also surface an alert for quick debugging
        alert('Lỗi khi tạo phiếu nhập. ' + serverMsg + ' (Xem console để biết chi tiết)');
      }
    });
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
    document.body.style.overflow = this.showFilters ? 'hidden' : '';
  }

  hasActiveFilters(): boolean {
    return this.filters.searchText !== '' ||
           this.filters.supplier !== '' ||
           this.filters.status !== '' ||
           this.filters.dateFrom !== '' ||
           this.filters.dateTo !== '' ||
           this.filters.sortBy !== 'date-desc';
  }

  getActiveFilterCount(): number {
    let count = 0;
    if (this.filters.searchText) count++;
    if (this.filters.supplier) count++;
    if (this.filters.status) count++;
    if (this.filters.dateFrom) count++;
    if (this.filters.dateTo) count++;
    if (this.filters.sortBy !== 'date-desc') count++;
    return count;
  }

  resetFilters(): void {
    this.filters = { searchText: '', supplier: '', status: '', dateFrom: '', dateTo: '', sortBy: 'date-desc' };
    this.applyFilters();
    if (window.innerWidth < 1024) {
      this.showFilters = false;
      document.body.style.overflow = '';
    }
  }

  applyFilters(): void {
    let result = [...this.stockIns];

    if (this.filters.searchText) {
      const q = this.filters.searchText.toLowerCase();
      result = result.filter(i => (i.code || '').toLowerCase().includes(q) || (i.supplier || '').toLowerCase().includes(q));
    }

    if (this.filters.supplier) {
      result = result.filter(i => (i.supplier || '').includes(this.filters.supplier));
    }

    if (this.filters.status) {
      result = result.filter(i => (i.status || 'Hoàn tất') === this.filters.status);
    }

    if (this.filters.dateFrom) {
      const from = new Date(this.filters.dateFrom).getTime();
      result = result.filter(i => new Date(i.date).getTime() >= from);
    }
    if (this.filters.dateTo) {
      const to = new Date(this.filters.dateTo).getTime();
      result = result.filter(i => new Date(i.date).getTime() <= to);
    }

    if (this.filters.sortBy === 'date-desc') {
      result.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
    } else if (this.filters.sortBy === 'date-asc') {
      result.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
    }

    this.filtered = result;
    console.log('✅ applyFilters -> filtered length:', this.filtered.length);
    if (window.innerWidth < 1024) {
      this.showFilters = false;
      document.body.style.overflow = '';
    }
  }
}
