import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InvoiceService } from '../services/invoice.service';
import { ProductService } from '../services/product.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface SalesData {
  date: string;
  revenue: number;
  orders: number;
}

interface TopProduct {
  productName: string;
  quantity: number;
  revenue: number;
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css']
})
export class ReportsComponent implements OnInit {
  // Date range
  startDate: string = '';
  endDate: string = '';
  
  // Summary stats
  totalRevenue: number = 0;
  totalOrders: number = 0;
  totalProducts: number = 0;
  averageOrderValue: number = 0;
  
  // Charts data
  salesData: SalesData[] = [];
  topProducts: TopProduct[] = [];
  bestSellingProducts: any[] = [];
  
  // Loading
  isLoading: boolean = false;
  
  // Filter
  selectedPeriod: 'today' | 'week' | 'month' | 'custom' = 'week';
  
  // Inventory summary
  inventorySummary: any = {
    totalProducts: 0,
    totalStock: 0,
    lowStockProducts: 0,
    outOfStockProducts: 0,
    totalInventoryValue: 0
  };

  // Customer summary
  customerSummary: any = {
    totalCustomers: 0,
    newCustomersThisMonth: 0,
    topCustomers: []
  };

  showExportMenu: boolean = false;

  constructor(
    private router: Router,
    private invoiceService: InvoiceService,
    private productService: ProductService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.initializeDateRange();
    this.loadReports();
    this.loadInventorySummary();
    this.loadCustomerSummary();
  }

  initializeDateRange(): void {
    const today = new Date();
    this.endDate = this.formatDate(today);
    
    const weekAgo = new Date(today);
    weekAgo.setDate(weekAgo.getDate() - 7);
    this.startDate = this.formatDate(weekAgo);
  }

  formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  onPeriodChange(period: 'today' | 'week' | 'month' | 'custom'): void {
    this.selectedPeriod = period;
    const today = new Date();
    this.endDate = this.formatDate(today);
    
    switch(period) {
      case 'today':
        this.startDate = this.formatDate(today);
        break;
      case 'week':
        const weekAgo = new Date(today);
        weekAgo.setDate(weekAgo.getDate() - 7);
        this.startDate = this.formatDate(weekAgo);
        break;
      case 'month':
        const monthAgo = new Date(today);
        monthAgo.setDate(monthAgo.getDate() - 30);
        this.startDate = this.formatDate(monthAgo);
        break;
      case 'custom':
        // User will manually select dates
        return;
    }
    
    this.loadReports();
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  loadReports(): void {
    this.isLoading = true;
    
    // Load all invoices
    this.invoiceService.getAllInvoices().subscribe({
      next: (response: any) => {
        const invoices = Array.isArray(response) ? response : (response.data || []);
        this.processInvoices(invoices);
        this.isLoading = false;
        
        // Sau khi xử lý xong invoices, load best selling products từ API
        this.loadBestSellingProducts();
      },
      error: (error) => {
        console.error('Error loading reports:', error);
        this.isLoading = false;
        // Nếu lỗi vẫn load best selling products
        this.loadBestSellingProducts();
      }
    });
  }

  processInvoices(invoices: any[]): void {
    const start = new Date(this.startDate);
    const end = new Date(this.endDate);
    end.setHours(23, 59, 59, 999);
    
    // Filter invoices by date range
    const filteredInvoices = invoices.filter(inv => {
      const invDate = new Date(inv.invoiceDate || inv.createdAt);
      return invDate >= start && invDate <= end;
    });
    
    // Calculate summary stats
    this.totalOrders = filteredInvoices.length;
    this.totalRevenue = filteredInvoices.reduce((sum, inv) => 
      sum + (inv.finalAmount || inv.totalAmount || 0), 0);
    this.averageOrderValue = this.totalOrders > 0 
      ? this.totalRevenue / this.totalOrders 
      : 0;
    
    // Group by date for chart
    this.salesData = this.groupByDate(filteredInvoices);
    
    // KHÔNG gọi calculateTopProducts nữa vì sẽ dùng API
    // this.calculateTopProducts(filteredInvoices);
  }

  groupByDate(invoices: any[]): SalesData[] {
    const grouped = new Map<string, { revenue: number, orders: number }>();
    
    invoices.forEach(inv => {
      const date = new Date(inv.invoiceDate || inv.createdAt)
        .toISOString().split('T')[0];
      const existing = grouped.get(date) || { revenue: 0, orders: 0 };
      existing.revenue += inv.finalAmount || inv.totalAmount || 0;
      existing.orders += 1;
      grouped.set(date, existing);
    });
    
    return Array.from(grouped.entries())
      .map(([date, data]) => ({
        date,
        revenue: data.revenue,
        orders: data.orders
      }))
      .sort((a, b) => a.date.localeCompare(b.date));
  }

  loadBestSellingProducts() {
    console.log('🔍 Loading best selling products from API...');
    
    this.http.get<any>('http://localhost:5001/api/Report/GetBestSellingProducts?limit=5', { 
      headers: this.getHeaders() 
    }).subscribe({
      next: (response) => {
        console.log('✅ API Response:', response);
        console.log('📊 Data received:', response.data);
        console.log('📊 Data length:', response.data?.length);
        
        if (response.success && response.data && response.data.length > 0) {
          this.topProducts = response.data;
          console.log('✅ Top products assigned:', this.topProducts);
        } else {
          console.warn('⚠️ No real data available:', response.message);
          this.topProducts = [];
        }
      },
      error: (error) => {
        console.error('❌ Error loading best selling products:', error);
        this.topProducts = [];
      }
    });
  }

  loadInventorySummary() {
    console.log('📦 Loading inventory summary...');
    
    this.http.get<any>('http://localhost:5001/api/Report/GetInventorySummary', { 
      headers: this.getHeaders() 
    }).subscribe({
      next: (response) => {
        console.log('✅ Inventory summary response:', response);
        
        if (response.success && response.data) {
          this.inventorySummary = response.data;
          console.log('✅ Inventory summary loaded:', this.inventorySummary);
        } else {
          console.warn('⚠️ No inventory data:', response.message);
          // Set default values
          this.inventorySummary = {
            totalProducts: 0,
            totalStock: 0,
            lowStockProducts: 0,
            outOfStockProducts: 0,
            totalInventoryValue: 0
          };
        }
      },
      error: (error) => {
        console.error('❌ Error loading inventory summary:', error);
        console.error('Status:', error.status);
        console.error('Message:', error.message);
        
        // Set default values on error
        this.inventorySummary = {
          totalProducts: 0,
          totalStock: 0,
          lowStockProducts: 0,
          outOfStockProducts: 0,
          totalInventoryValue: 0
        };
      }
    });
  }

  loadCustomerSummary() {
    console.log('👥 Loading customer summary...');
    
    this.http.get<any>('http://localhost:5001/api/Report/GetCustomerSummary', { 
      headers: this.getHeaders() 
    }).subscribe({
      next: (response) => {
        console.log('✅ Customer summary response:', response);
        
        if (response.success && response.data) {
          this.customerSummary = response.data;
          console.log('✅ Customer summary loaded:', this.customerSummary);
        } else {
          console.warn('⚠️ No customer data:', response.message);
          // Set default values
          this.customerSummary = {
            totalCustomers: 0,
            newCustomersThisMonth: 0,
            topCustomers: []
          };
        }
      },
      error: (error) => {
        console.error('❌ Error loading customer summary:', error);
        console.error('Status:', error.status);
        console.error('Message:', error.message);
        
        // Set default values on error
        this.customerSummary = {
          totalCustomers: 0,
          newCustomersThisMonth: 0,
          topCustomers: []
        };
      }
    });
  }

  exportReport(): void {
    const exportData = {
      period: this.selectedPeriod,
      startDate: this.startDate,
      endDate: this.endDate,
      summary: {
        totalRevenue: this.totalRevenue,
        totalOrders: this.totalOrders,
        averageOrderValue: this.averageOrderValue
      },
      salesData: this.salesData,
      topProducts: this.topProducts
    };

    // Xuất dưới dạng CSV
    this.exportToCSV(exportData);
  }

  exportToCSV(data: any): void {
    let csvContent = 'data:text/csv;charset=utf-8,';
    
    // Header
    csvContent += 'BÁO CÁO BÁN HÀNG\n';
    csvContent += `Từ ngày: ${this.startDate} - Đến ngày: ${this.endDate}\n\n`;
    
    // Summary
    csvContent += 'TỔNG QUAN\n';
    csvContent += `Tổng doanh thu,${this.totalRevenue}\n`;
    csvContent += `Tổng đơn hàng,${this.totalOrders}\n`;
    csvContent += `Giá trị TB/đơn,${this.averageOrderValue}\n\n`;
    
    // Inventory
    csvContent += 'HÀNG HÓA\n';
    csvContent += `Tổng sản phẩm,${this.inventorySummary.totalProducts}\n`;
    csvContent += `Tổng tồn kho,${this.inventorySummary.totalStock}\n`;
    csvContent += `Sản phẩm sắp hết,${this.inventorySummary.lowStockProducts}\n`;
    csvContent += `Sản phẩm hết hàng,${this.inventorySummary.outOfStockProducts}\n`;
    csvContent += `Giá trị tồn kho,${this.inventorySummary.totalInventoryValue}\n\n`;
    
    // Customers
    csvContent += 'KHÁCH HÀNG\n';
    csvContent += `Tổng khách hàng,${this.customerSummary.totalCustomers}\n`;
    csvContent += `Khách hàng mới tháng này,${this.customerSummary.newCustomersThisMonth}\n\n`;
    
    // Sales by date
    csvContent += 'DOANH THU THEO NGÀY\n';
    csvContent += 'Ngày,Số đơn,Doanh thu,TB/đơn\n';
    this.salesData.forEach(item => {
      csvContent += `${item.date},${item.orders},${item.revenue},${item.revenue/item.orders}\n`;
    });
    
    // Top products
    csvContent += '\nTOP SẢN PHẨM BÁN CHẠY\n';
    csvContent += 'Thứ hạng,Tên sản phẩm,Số lượng,Doanh thu\n';
    this.topProducts.forEach((product, index) => {
      csvContent += `${index + 1},${product.productName},${product.quantity},${product.revenue}\n`;
    });
    
    // Download
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `bao-cao-ban-hang-${this.startDate}-${this.endDate}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    this.showExportMenu = false;
    console.log('✅ Báo cáo CSV đã được xuất thành công!');
  }

  exportToPDF(): void {
    this.showExportMenu = false;
    window.print();
  }

  exportToExcel(): void {
    let tableHTML = '<table border="1">';
    
    tableHTML += '<tr><th colspan="4" style="text-align:center;font-size:16px;font-weight:bold;">BÁO CÁO BÁN HÀNG</th></tr>';
    tableHTML += `<tr><th colspan="4">Từ ngày: ${this.startDate} - Đến ngày: ${this.endDate}</th></tr>`;
    tableHTML += '<tr><th colspan="4"></th></tr>';
    
    // Summary
    tableHTML += '<tr><th colspan="4" style="background:#f0f0f0;">TỔNG QUAN</th></tr>';
    tableHTML += `<tr><td>Tổng doanh thu</td><td colspan="3">${this.totalRevenue.toLocaleString()} đ</td></tr>`;
    tableHTML += `<tr><td>Tổng đơn hàng</td><td colspan="3">${this.totalOrders}</td></tr>`;
    tableHTML += `<tr><td>Giá trị TB/đơn</td><td colspan="3">${this.averageOrderValue.toLocaleString()} đ</td></tr>`;
    tableHTML += '<tr><th colspan="4"></th></tr>';
    
    // Inventory
    tableHTML += '<tr><th colspan="4" style="background:#f0f0f0;">HÀNG HÓA</th></tr>';
    tableHTML += `<tr><td>Tổng sản phẩm</td><td colspan="3">${this.inventorySummary.totalProducts}</td></tr>`;
    tableHTML += `<tr><td>Tổng tồn kho</td><td colspan="3">${this.inventorySummary.totalStock}</td></tr>`;
    tableHTML += `<tr><td>Sản phẩm sắp hết</td><td colspan="3">${this.inventorySummary.lowStockProducts}</td></tr>`;
    tableHTML += `<tr><td>Sản phẩm hết hàng</td><td colspan="3">${this.inventorySummary.outOfStockProducts}</td></tr>`;
    tableHTML += `<tr><td>Giá trị tồn kho</td><td colspan="3">${this.inventorySummary.totalInventoryValue.toLocaleString()} đ</td></tr>`;
    tableHTML += '<tr><th colspan="4"></th></tr>';
    
    // Customers
    tableHTML += '<tr><th colspan="4" style="background:#f0f0f0;">KHÁCH HÀNG</th></tr>';
    tableHTML += `<tr><td>Tổng khách hàng</td><td colspan="3">${this.customerSummary.totalCustomers}</td></tr>`;
    tableHTML += `<tr><td>Khách hàng mới tháng này</td><td colspan="3">${this.customerSummary.newCustomersThisMonth}</td></tr>`;
    tableHTML += '<tr><th colspan="4"></th></tr>';
    
    // Sales data and top products...
    tableHTML += '</table>';
    
    const blob = new Blob([tableHTML], { type: 'application/vnd.ms-excel' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `bao-cao-ban-hang-${this.startDate}-${this.endDate}.xls`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    this.showExportMenu = false;
    console.log('✅ Báo cáo Excel đã được xuất thành công!');
  }

  toggleExportMenu(): void {
    this.showExportMenu = !this.showExportMenu;
  }

  navigateToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.export-dropdown')) {
      this.showExportMenu = false;
    }
  }
}
