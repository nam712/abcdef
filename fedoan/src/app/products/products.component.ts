import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductCategoryService } from '../services/product-category.service';
import { SupplierService } from '../services/supplier.service';
import { ProductService } from '../services/product.service';
import { HttpClient, HttpHeaders } from '@angular/common/http'; // Thêm dòng này nếu chưa có
import { environment } from '../../environments/environment';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { NotificationService } from '../services/notification.service';
import { InventoryAlertService } from '../services/inventory-alert.service'; 
import { AuthService } from '../services/auth.service';

export interface Product {
  productCode: string;
  productName: string;
  description?: string;
  categoryId: number;
  brand?: string;
  supplierId: number;
  price: number;
  costPrice: number;
  stock: number;
  minStock: number;
  sku: string;
  barcode?: string;
  unit: string;
  imageUrl?: string | null;
  notes?: string;
  weight?: number;
  dimension?: string;
  status?: string;
}

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NotificationBellComponent],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.css']
})
export class ProductsComponent implements OnInit, OnDestroy {
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
  
  categories: any[] = [];
  suppliers: any[] = [];
  units = ['Cái', 'Hộp', 'Thùng', 'Kg', 'Lít', 'Chai', 'Gói', 'Bộ', 'Chiếc'];

  products: Product[] = [];
  filteredProducts: Product[] = [];
  
  showDialog = false;
  isEditMode = false;
  currentProduct: Product = this.getEmptyProduct();
  currentProductId: number | null = null;
  isLoading = false;
  errorMessage = '';
  showFilters = false;
  showCategoryDialog = false;
  newCategoryName = '';

  filters = {
    searchText: '',
    category: '',
    status: '',
    priceRange: '',
    stockLevel: '',
    sortBy: 'name-asc'
  };

  selectedFile: File | null = null; // Thêm dòng này nếu chưa có

  constructor(
    private router: Router,
    private categoryService: ProductCategoryService,
    private supplierService: SupplierService,
    private productService: ProductService,
    private http: HttpClient,
    private notificationService: NotificationService,
    private inventoryAlertService: InventoryAlertService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    console.log('✅ Products component loaded!');
    const savedTheme = localStorage.getItem('dashboard-theme');
    if (savedTheme) {
      this.currentTheme = savedTheme;
      this.applyTheme(savedTheme);
    }
    this.loadCategories();
    this.loadSuppliers();
    this.loadProducts();
  }

  ngOnDestroy(): void {
    document.body.style.overflow = '';
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
    this.closeMobileMenu();
    this.router.navigate(['/dashboard']);
  }

  navigateToProducts(): void {
    this.closeMobileMenu();
    this.closeProductsSubmenu();
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

  // Product methods
  private getEmptyProduct(): Product {
    return {
      productCode: this.generateProductCode(),
      productName: '',
      description: '',
      categoryId: 0,
      brand: '',
      supplierId: 0,
      price: 0,
      costPrice: 0,
      stock: 0,
      minStock: 0,
      sku: '',
      barcode: this.generateBarcode(), // Auto-generate unique barcode
      unit: 'Cái',
      imageUrl: null,
      notes: '',
      weight: 0,
      dimension: '',
      status: 'active'
    };
  }

  private generateProductCode(): string {
    // Generate a mini GUID (8 characters)
    return 'xxxxxxxx'.replace(/[x]/g, function(c) {
      const r = Math.random() * 16 | 0;
      return r.toString(16);
    });
  }

  private generateBarcode(): string {
    // Generate unique 13-digit barcode
    const timestamp = Date.now().toString();
    const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
    return (timestamp + random).slice(-13);
  }

  loadCategories(): void {
    console.log('📂 Loading categories from API...');
    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        console.log('✅ Categories response:', response);
        
        if (response && response.success && response.data) {
          let categoriesData = response.data;
          
          if (Array.isArray(categoriesData)) {
            this.categories = categoriesData.map((cat: any) => ({
              categoryId: cat.categoryId || cat.CategoryId,
              categoryName: cat.categoryName || cat.CategoryName
            }));
          }
          
          console.log(`✅ Loaded ${this.categories.length} categories:`, this.categories);
        }
      },
      error: (error) => {
        console.error('❌ Error loading categories:', error);
        this.categories = [
          { categoryId: 1, categoryName: 'Điện tử' },
          { categoryId: 2, categoryName: 'Thực phẩm' },
          { categoryId: 3, categoryName: 'Đồ uống' }
        ];
      }
    });
  }

  loadSuppliers(): void {
    console.log('🏢 Loading suppliers from API...');
    
    this.supplierService.getAllSuppliers().subscribe({
      next: (response) => {
        console.log('✅ Raw Suppliers response:', response);
        
        if (response && response.success && response.data) {
          const data = response.data as any;
          
          if (Array.isArray(data)) {
            this.suppliers = data.map((sup: any) => ({
              supplierId: sup.supplierId || sup.id || sup.Id,
              supplierName: sup.supplierName || sup.SupplierName,
              supplierCode: sup.supplierCode || sup.SupplierCode
            }));
          } else if (data.suppliers && Array.isArray(data.suppliers)) {
            this.suppliers = data.suppliers.map((sup: any) => ({
              supplierId: sup.supplierId || sup.id || sup.Id,
              supplierName: sup.supplierName || sup.SupplierName,
              supplierCode: sup.supplierCode || sup.SupplierCode
            }));
          } else if (data.items && Array.isArray(data.items)) {
            this.suppliers = data.items.map((sup: any) => ({
              supplierId: sup.supplierId || sup.id || sup.Id,
              supplierName: sup.supplierName || sup.SupplierName,
              supplierCode: sup.supplierCode || sup.SupplierCode
            }));
          }
          
          console.log(`✅ Loaded ${this.suppliers.length} suppliers`);
        }
      },
      error: (error) => {
        console.error('❌ Error loading suppliers:', error);
        this.suppliers = [
          { supplierId: 1, supplierName: 'Công ty Test 1', supplierCode: 'TEST-001' },
          { supplierId: 2, supplierName: 'Công ty Test 2', supplierCode: 'TEST-002' }
        ];
      }
    });
  }

  loadProducts(): void {
    this.isLoading = true;
    console.log('📦 Loading products from API...');
    console.log('🌐 API URL:', 'http://localhost:5001/api/Product/GetAll');
    
    this.productService.getAllProducts().subscribe({
      next: (response) => {
        console.log('✅ Raw Products response:', response);
        console.log('📊 Response structure:', JSON.stringify(response, null, 2));
        
        if (response && response.success && response.data) {
          const data = response.data;
          console.log('📦 Data type:', typeof data);
          console.log('📦 Is Array?', Array.isArray(data));
          
          if (Array.isArray(data)) {
            console.log('✅ Data is Array with', data.length, 'items');
            this.products = data.map((prod: any) => ({
              productId: prod.productId || prod.ProductId,
              productCode: prod.productCode || prod.ProductCode,
              productName: prod.productName || prod.ProductName,
              description: prod.description || prod.Description,
              categoryId: prod.categoryId || prod.CategoryId || 0,
              brand: prod.brand || prod.Brand,
              supplierId: prod.supplierId || prod.SupplierId || 0,
              price: prod.price || prod.Price || 0,
              costPrice: prod.costPrice || prod.CostPrice || 0,
              stock: prod.stock || prod.Stock || 0,
              minStock: prod.minStock || prod.MinStock || 0,
              sku: prod.sku || prod.SKU || prod.Sku || '',
              barcode: prod.barcode || prod.Barcode,
              unit: prod.unit || prod.Unit || 'Cái',
              imageUrl: prod.imageUrl || prod.ImageUrl,
              notes: prod.notes || prod.Notes,
              weight: prod.weight || prod.Weight,
              dimension: prod.dimension || prod.Dimension,
              status: prod.status || prod.Status || 'active'
            }));
            console.log(`✅ Mapped ${this.products.length} products successfully`);
          } else {
            console.error('❌ Data is not an array:', data);
            this.products = [];
          }
          
          this.applyFilters();
        } else {
          console.error('❌ Invalid response structure:', response);
          this.products = [];
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ HTTP Error loading products');
        console.error('Status:', error.status);
        console.error('Status Text:', error.statusText);
        console.error('URL:', error.url);
        console.error('Message:', error.message);
        console.error('Error object:', error);
        
        if (error.status === 500) {
          this.errorMessage = '❌ Lỗi server (500). Kiểm tra backend console để xem chi tiết lỗi.';
        } else if (error.status === 0) {
          this.errorMessage = '❌ Không thể kết nối đến API server. Kiểm tra API có đang chạy không.';
        } else if (error.status === 404) {
          this.errorMessage = '❌ API endpoint không tồn tại: ' + error.url;
        } else {
          this.errorMessage = `❌ Lỗi ${error.status}: ${error.message}`;
        }
        
        // Use empty array instead of crashing
        this.products = [];
        this.filteredProducts = [];
        this.isLoading = false;
      }
    });
  }

  getCategoryName(categoryId: number): string {
    const category = this.categories.find(c => c.categoryId === categoryId);
    return category?.categoryName || 'N/A';
  }

  getStockStatus(product: Product): string {
    if (product.stock < product.minStock) return 'low';
    if (product.stock > product.minStock * 2) return 'high';
    return 'normal';
  }

  applyFilters(): void {
    let result = [...this.products];

    if (this.filters.searchText) {
      const searchLower = this.filters.searchText.toLowerCase();
      result = result.filter(p => 
        p.productName?.toLowerCase().includes(searchLower) ||
        p.productCode?.toLowerCase().includes(searchLower) ||
        p.sku?.toLowerCase().includes(searchLower) ||
        p.brand?.toLowerCase().includes(searchLower)
      );
    }

    if (this.filters.category) {
      result = result.filter(p => {
        const cat = this.categories.find(c => c.categoryId === p.categoryId);
        return cat?.categoryName === this.filters.category;
      });
    }

    if (this.filters.status) {
      result = result.filter(p => p.status === this.filters.status);
    }

    if (this.filters.stockLevel) {
      result = result.filter(p => {
        if (this.filters.stockLevel === 'low') return p.stock < p.minStock;
        if (this.filters.stockLevel === 'normal') return p.stock >= p.minStock && p.stock <= p.minStock * 2;
        if (this.filters.stockLevel === 'high') return p.stock > p.minStock * 2;
        return true;
      });
    }

    switch (this.filters.sortBy) {
      case 'name-asc':
        result.sort((a, b) => (a.productName || '').localeCompare(b.productName || ''));
        break;
      case 'name-desc':
        result.sort((a, b) => (b.productName || '').localeCompare(a.productName || ''));
        break;
      case 'price-asc':
        result.sort((a, b) => a.price - b.price);
        break;
      case 'price-desc':
        result.sort((a, b) => b.price - a.price);
        break;
    }

    this.filteredProducts = result;
  }

  resetFilters(): void {
    this.filters = {
      searchText: '',
      category: '',
      status: '',
      priceRange: '',
      stockLevel: '',
      sortBy: 'name-asc'
    };
    this.applyFilters();
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
           this.filters.category !== '' ||
           this.filters.status !== '' ||
           this.filters.stockLevel !== '' ||
           this.filters.sortBy !== 'name-asc';
  }

  getActiveFilterCount(): number {
    let count = 0;
    if (this.filters.searchText) count++;
    if (this.filters.category) count++;
    if (this.filters.status) count++;
    if (this.filters.stockLevel) count++;
    if (this.filters.sortBy !== 'name-asc') count++;
    return count;
  }

  openAddDialog(): void {
    this.isEditMode = false;
    this.currentProduct = this.getEmptyProduct();
    this.currentProduct.productCode = this.generateProductCode(); // Tạo mã mới mỗi lần mở dialog
    this.currentProductId = null;
    this.showDialog = true;
    this.errorMessage = '';
  }

  openCategoryDialog(): void {
    console.log('📂 Opening category dialog...');
    this.newCategoryName = '';
    this.showCategoryDialog = true;
  }

  closeCategoryDialog(): void {
    this.showCategoryDialog = false;
    this.newCategoryName = '';
  }

  saveCategory(): void {
    if (!this.newCategoryName || this.newCategoryName.trim() === '') {
      alert('❌ Vui lòng nhập tên danh mục!');
      return;
    }

    const trimmedName = this.newCategoryName.trim();

    if (this.categories.some(c => c.categoryName === trimmedName)) {
      alert('❌ Danh mục này đã tồn tại!');
      return;
    }

    console.log('💾 Saving category to API:', trimmedName);
    this.isLoading = true;

    this.categoryService.createCategory(trimmedName).subscribe({
      next: (response) => {
        console.log('✅ Create category response:', response);
        
        if (response.success && response.data) {
          const newCategory = {
            categoryId: response.data.categoryId || response.data.CategoryId,
            categoryName: response.data.categoryName || response.data.CategoryName
          };
          
          this.categories.push(newCategory);
          this.categories.sort((a, b) => a.categoryName.localeCompare(b.categoryName));
          this.currentProduct.categoryId = newCategory.categoryId;

          alert(`✅ Đã thêm danh mục "${trimmedName}" thành công!`);
          this.closeCategoryDialog();
        }
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Error creating category:', error);
        alert('❌ Có lỗi xảy ra: ' + (error.error?.message || error.message));
        this.isLoading = false;
      }
    });
  }

  viewProduct(product: Product): void {
    const cat = this.categories.find(c => c.categoryId === product.categoryId);
    const sup = this.suppliers.find(s => s.supplierId === product.supplierId);
    
    const details = `
━━━━━━━━━━━━━━━━━━━━━━━━━━━
📦 THÔNG TIN SẢN PHẨM
━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔖 Mã SP: ${product.productCode}
📋 SKU: ${product.sku}
📦 Tên: ${product.productName}

📁 Danh mục: ${cat?.categoryName || 'N/A'}
🏭 Thương hiệu: ${product.brand || 'N/A'}
🏢 NCC: ${sup?.supplierName || 'N/A'}

💰 Giá vốn: ${product.costPrice.toLocaleString('vi-VN')} ₫
💵 Giá bán: ${product.price.toLocaleString('vi-VN')} ₫

📊 Tồn kho: ${product.stock}
⚠️ Tồn tối thiểu: ${product.minStock}
📦 Đơn vị: ${product.unit}

🔢 Barcode: ${product.barcode || 'N/A'}
⚖️ Khối lượng: ${product.weight || 'N/A'} kg
📏 Kích thước: ${product.dimension || 'N/A'}

📝 Mô tả: ${product.description || 'Không có'}
📌 Ghi chú: ${product.notes || 'Không có'}
🎯 Trạng thái: ${product.status === 'active' ? 'Hoạt động' : 'Ngừng KD'}
    `;
    alert(details);
  }

  editProduct(product: Product): void {
    this.isEditMode = true;
    this.currentProduct = { ...product };
    this.currentProductId = (product as any).productId;
    this.showDialog = true;
    this.errorMessage = '';
  }

  saveProduct(): void {
    console.log('💾 Saving product...', this.currentProduct);
    this.errorMessage = '';
    
    // Validation
    if (!this.currentProduct.productName || this.currentProduct.productName.trim() === '') {
      this.errorMessage = 'Vui lòng nhập tên sản phẩm!';
      return;
    }
    
    if (!this.currentProduct.sku || this.currentProduct.sku.trim() === '') {
      this.errorMessage = 'Vui lòng nhập SKU!';
      return;
    }
    
    if (this.currentProduct.categoryId === 0) {
      this.errorMessage = 'Vui lòng chọn danh mục!';
      return;
    }
    
    if (this.currentProduct.supplierId === 0) {
      this.errorMessage = 'Vui lòng chọn nhà cung cấp!';
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

    this.isLoading = true;

    // Prepare product data
    const productData = {
      productCode: this.currentProduct.productCode || this.generateProductCode(),
      productName: this.currentProduct.productName?.trim() || '',
      description: this.currentProduct.description?.trim() || '',
      categoryId: Number(this.currentProduct.categoryId),
      brand: this.currentProduct.brand?.trim() || '',
      supplierId: Number(this.currentProduct.supplierId),
      price: Number(this.currentProduct.price) || 0,
      costPrice: Number(this.currentProduct.costPrice) || 0,
      stock: Number(this.currentProduct.stock) || 0,
      minStock: Number(this.currentProduct.minStock) || 0,
      sku: this.currentProduct.sku?.trim() || '',
      barcode: this.currentProduct.barcode?.trim() || this.generateBarcode(),
      unit: this.currentProduct.unit || 'Cái',
      notes: this.currentProduct.notes?.trim() || '',
      weight: this.currentProduct.weight ? Number(this.currentProduct.weight) : 0,
      dimension: this.currentProduct.dimension?.trim() || '',
      status: this.currentProduct.status || 'active',
      shop_owner_id: parseInt(shopOwnerId, 10)
    };

    console.log('📤 Sending product data:', productData);

    if (this.isEditMode && this.currentProductId) {
      // Update - Backend expects FormData, so always use FormData
      const formData = new FormData();
      
      // Append all fields
      formData.append('productCode', productData.productCode);
      formData.append('productName', productData.productName);
      formData.append('description', productData.description);
      formData.append('categoryId', productData.categoryId.toString());
      formData.append('brand', productData.brand);
      formData.append('supplierId', productData.supplierId.toString());
      formData.append('price', productData.price.toString());
      formData.append('costPrice', productData.costPrice.toString());
      formData.append('stock', productData.stock.toString());
      formData.append('minStock', productData.minStock.toString());
      formData.append('sku', productData.sku);
      formData.append('barcode', productData.barcode);
      formData.append('unit', productData.unit);
      formData.append('notes', productData.notes);
      formData.append('weight', productData.weight.toString());
      formData.append('dimension', productData.dimension);
      formData.append('status', productData.status);
      formData.append('shop_owner_id', productData.shop_owner_id.toString());

      // If there's a new image file, append it
      if (this.selectedFile) {
        formData.append('image', this.selectedFile, this.selectedFile.name);
      }

      // Get token
      const token = localStorage.getItem('auth_token') || localStorage.getItem('access_token');
      const headers = new HttpHeaders({
        'Authorization': token ? `Bearer ${token}` : ''
        // DO NOT set Content-Type - let browser set it automatically for multipart/form-data
      });

      this.http.put(
        `${environment.apiUrl}/api/Product/Update/${this.currentProductId}`,
        formData,
        { headers }
      ).subscribe({
        next: (response: any) => {
          this.handleUpdateSuccess(response, productData);
        },
        error: (error) => {
          console.error('❌ Update error:', error);
          this.handleError(error);
          this.isLoading = false;
        }
      });
    } else {
      // Create - use JSON
      this.productService.createProduct(productData).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('✅ Create response:', response);
            
            this.notificationService.addNotification(
              `Đã thêm sản phẩm "${productData.productName}" thành công!`, 
              'success',
              {
                entityType: 'Product',
                entityId: response.data?.productId,
                action: 'Create',
                route: '/products'
              }
            );
            
            if (response.data && response.data.productId) {
              this.inventoryAlertService.checkProduct({
                productId: response.data.productId,
                productName: productData.productName,
                stock: productData.stock,
                minimumStock: productData.minStock,
                productCode: productData.productCode
              });
            }
            
            this.loadProducts();
            this.closeDialog();
          } else {
            this.errorMessage = response.message || 'Thêm mới thất bại';
            this.notificationService.addNotification(
              response.message || 'Thêm mới thất bại!', 
              'error'
            );
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

  private handleUpdateSuccess(response: any, productData: any): void {
    if (response.success) {
      console.log('✅ Update response:', response);
      
      this.notificationService.addNotification(
        `Đã cập nhật sản phẩm "${productData.productName}" thành công!`, 
        'success',
        {
          entityType: 'Product',
          entityId: this.currentProductId!,
          action: 'Update',
          route: '/products'
        }
      );
      
      // Optimized: Update in-place without reloading entire list
      if (response.data) {
        const index = this.products.findIndex(p => (p as any).productId === this.currentProductId);
        if (index !== -1) {
          this.products[index] = {
            ...this.products[index],
            productCode: response.data.productCode || productData.productCode,
            productName: response.data.productName || productData.productName,
            description: response.data.description || productData.description,
            categoryId: response.data.categoryId || productData.categoryId,
            brand: response.data.brand || productData.brand,
            supplierId: response.data.supplierId || productData.supplierId,
            price: response.data.price || productData.price,
            costPrice: response.data.costPrice || productData.costPrice,
            stock: response.data.stock || productData.stock,
            minStock: response.data.minStock || productData.minStock,
            sku: response.data.sku || productData.sku,
            barcode: response.data.barcode || productData.barcode,
            unit: response.data.unit || productData.unit,
            imageUrl: response.data.imageUrl || this.currentProduct.imageUrl,
            notes: response.data.notes || productData.notes,
            weight: response.data.weight || productData.weight,
            dimension: response.data.dimension || productData.dimension,
            status: response.data.status || productData.status
          };
          (this.products[index] as any).productId = this.currentProductId;
          
          // Re-apply filters without API call
          this.applyFilters();
        }
      }
      
      // Check inventory alert
      this.inventoryAlertService.checkProduct({
        productId: this.currentProductId!,
        productName: productData.productName,
        stock: productData.stock,
        minimumStock: productData.minStock,
        productCode: productData.productCode
      });
      
      this.closeDialog();
    } else {
      this.errorMessage = response.message || 'Cập nhật thất bại';
      this.notificationService.addNotification(
        response.message || 'Cập nhật sản phẩm thất bại!', 
        'error'
      );
    }
    this.isLoading = false;
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
      this.errorMessage = '❌ Lỗi server (500). Vui lòng kiểm tra backend logs.';
      this.notificationService.addNotification('Lỗi server!', 'error');
    } else {
      this.errorMessage = error.error?.message || error.message || 'Có lỗi xảy ra';
      this.notificationService.addNotification(this.errorMessage, 'error');
    }
  }

  deleteProduct(product: Product): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa sản phẩm "${product.productName}"?`)) return;
    
    const productId = (product as any).productId;
    if (!productId) {
      this.notificationService.addNotification('Không tìm thấy ID sản phẩm!', 'error');
      return;
    }

    this.isLoading = true;
    this.productService.deleteProduct(productId).subscribe({
      next: (response) => {
        if (response.success) {
          console.log('✅ Delete response:', response);
          
          this.notificationService.addNotification(
            `Đã xóa sản phẩm "${product.productName}" thành công!`, 
            'success',
            {
              entityType: 'Product',
              entityId: productId,
              action: 'Delete',
              route: '/products'
            }
          );
          
          this.loadProducts();
        } else {
          this.notificationService.addNotification(
            response.message || 'Xóa sản phẩm thất bại!', 
            'error'
          );
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('❌ Delete error:', error);
        this.handleError(error);
        this.isLoading = false;
      }
    });
  }

  closeDialog(): void {
    this.showDialog = false;
    this.selectedFile = null;
    document.body.style.overflow = '';
  }

  trackByProductCode(index: number, product: Product): string {
    return product.productCode;
  }

  onImageFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.errorMessage = 'Vui lòng chọn file hình ảnh!';
        return;
      }
      
      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.errorMessage = 'Kích thước file không được vượt quá 5MB!';
        return;
      }
      
      // Store the file for later upload
      this.selectedFile = file;
      
      // Show preview
      const reader = new FileReader();
      reader.onload = () => {
        this.currentProduct.imageUrl = reader.result as string;
        console.log('✅ Image preview loaded');
      };
      reader.readAsDataURL(file);
    }
  }

  getImageUrl(imageUrl: string): string {
    if (!imageUrl) return '';
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) return imageUrl;
    if (imageUrl.startsWith('data:image')) return imageUrl;
    return `${environment.apiUrl}${imageUrl}`;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
