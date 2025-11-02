import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'home',
    loadComponent: () => import('./home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  
  {
    path: 'dashboard',
    loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'products',
    loadComponent: () => import('./products/products.component').then(m => m.ProductsComponent)
  },
  {
    path: 'customers',
    loadComponent: () => import('./customers/customers.component').then(m => m.CustomersComponent)
  },
  {
    path: 'pos',
    loadComponent: () => import('./pos/pos.component').then(m => m.PosComponent)
  },
  {
    path: 'manufacturer',
    loadComponent: () => import('./manufacturer/manufacturer.component').then(m => m.ManufacturerComponent)
  },
  {
    path: 'stock-in',
    loadComponent: () => import('./stock-in/stock-in.component').then(m => m.StockInComponent)
  },
  {
    path: 'invoices',
    loadComponent: () => import('./invoice/invoice.component').then(m => m.InvoiceComponent)
  },
  {
    path: 'employees',
    loadComponent: () => import('./employees/employees.component').then(m => m.EmployeesComponent)
  },
  {
    path: 'purchase-orders',
    loadComponent: () => import('./invoice/invoice.component').then(m => m.InvoiceComponent)
  },
  // {
  //   path: 'reports',
  //   loadComponent: () => import('./reports/reports.component').then(m => m.ReportsComponent)
  // },
  { path: '**', redirectTo: '/login' }
];

// Nếu bạn gặp lỗi "Could not resolve", hãy kiểm tra:
// - Đã tạo file employees/employees.component.ts và reports/reports.component.ts chưa?
// - Đã export EmployeesComponent và ReportsComponent đúng tên chưa?
// Nếu chưa có file, hãy tạo mới các file component này.
