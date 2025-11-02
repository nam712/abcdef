import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NotificationService } from './notification.service';
import { environment } from '../../environments/environment';

export interface Product {
  productId: number;
  productName: string;
  stock: number;
  minimumStock: number;
  productCode?: string;
}

@Injectable({
  providedIn: 'root'
})
export class InventoryAlertService {
  private apiUrl = `${environment.apiUrl}/api/Product/GetAll`; // ✅ FIX: Đổi từ /api/Products → /api/Product/GetAll
  private alertedProducts = new Set<number>(); // Lưu các sản phẩm đã cảnh báo để tránh spam
  private checkInterval = 60000; // Kiểm tra mỗi 60 giây
  private intervalId: any;

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {}

  /**
   * Bắt đầu theo dõi tồn kho
   */
  startMonitoring(): void {
    console.log('🔍 Bắt đầu theo dõi tồn kho...');
    
    // Kiểm tra ngay lập tức
    this.checkInventory();
    
    // Sau đó kiểm tra định kỳ
    this.intervalId = setInterval(() => {
      this.checkInventory();
    }, this.checkInterval);
  }

  /**
   * Dừng theo dõi tồn kho
   */
  stopMonitoring(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      console.log('🛑 Đã dừng theo dõi tồn kho');
    }
  }

  /**
   * Kiểm tra tồn kho tất cả sản phẩm
   */
  private checkInventory(): void {
    this.http.get<any>(this.apiUrl).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const products = response.data as Product[];
          this.checkLowStockProducts(products);
        }
      },
      error: (error) => {
        console.error('❌ Lỗi khi kiểm tra tồn kho:', error);
      }
    });
  }

  /**
   * Kiểm tra sản phẩm sắp hết hàng
   */
  private checkLowStockProducts(products: Product[]): void {
    products.forEach(product => {
      // Kiểm tra nếu stock <= minimumStock
      if (product.stock <= product.minimumStock && product.stock > 0) {
        // Chỉ cảnh báo nếu chưa được cảnh báo trước đó
        if (!this.alertedProducts.has(product.productId)) {
          this.createLowStockAlert(product);
          this.alertedProducts.add(product.productId);
          
          // Xóa khỏi danh sách đã cảnh báo sau 24 giờ
          setTimeout(() => {
            this.alertedProducts.delete(product.productId);
          }, 24 * 60 * 60 * 1000);
        }
      } else if (product.stock > product.minimumStock) {
        // Nếu đã nhập thêm hàng, cho phép cảnh báo lại
        this.alertedProducts.delete(product.productId);
      }

      // Cảnh báo nếu hết hàng hoàn toàn
      if (product.stock === 0) {
        if (!this.alertedProducts.has(product.productId)) {
          this.createOutOfStockAlert(product);
          this.alertedProducts.add(product.productId);
          
          setTimeout(() => {
            this.alertedProducts.delete(product.productId);
          }, 24 * 60 * 60 * 1000);
        }
      }
    });
  }

  /**
   * Tạo cảnh báo sản phẩm sắp hết hàng
   */
  private createLowStockAlert(product: Product): void {
    const message = `⚠️ Sản phẩm "${product.productName}" sắp hết hàng! (Còn ${product.stock} / Tối thiểu ${product.minimumStock})`;
    
    this.notificationService.addNotification(
      message,
      'warning',
      {
        entityType: 'Product',
        entityId: product.productId,
        action: 'LowStock',
        metadata: {
          productCode: product.productCode,
          productName: product.productName,
          currentStock: product.stock,
          minimumStock: product.minimumStock
        },
        route: '/products'
      }
    );

    console.log('⚠️ Cảnh báo tồn kho thấp:', product.productName);
  }

  /**
   * Tạo cảnh báo sản phẩm hết hàng
   */
  private createOutOfStockAlert(product: Product): void {
    const message = `🚨 Sản phẩm "${product.productName}" đã hết hàng! Cần nhập hàng ngay.`;
    
    this.notificationService.addNotification(
      message,
      'error',
      {
        entityType: 'Product',
        entityId: product.productId,
        action: 'OutOfStock',
        metadata: {
          productCode: product.productCode,
          productName: product.productName,
          minimumStock: product.minimumStock
        },
        route: '/products'
      }
    );

    console.log('🚨 Cảnh báo hết hàng:', product.productName);
  }

  /**
   * Kiểm tra một sản phẩm cụ thể (gọi khi cập nhật số lượng)
   */
  checkProduct(product: Product): void {
    if (product.stock <= product.minimumStock && product.stock > 0) {
      this.createLowStockAlert(product);
    } else if (product.stock === 0) {
      this.createOutOfStockAlert(product);
    }
  }

  /**
   * Reset danh sách đã cảnh báo (dùng khi cần)
   */
  resetAlerts(): void {
    this.alertedProducts.clear();
    console.log('🔄 Đã reset danh sách cảnh báo');
  }
}
