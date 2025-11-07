import { Routes } from '@angular/router';
import { AuthGuard } from './auth.guard';
import { LoginComponent } from './login/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ProductsComponent } from './products/products.component';
import { PosComponent } from './pos/pos.component';
import { CustomersComponent } from './customers/customers.component';
import { EmployeesComponent } from './employees/employees.component';
import { ReportsComponent } from './reports/reports.component';
import { ManufacturerComponent } from './manufacturer/manufacturer.component';
import { InvoiceComponent } from './invoice/invoice.component';
import { PromotionsComponent } from './promotions/promotions.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'products', component: ProductsComponent, canActivate: [AuthGuard] },
  { path: 'pos', component: PosComponent, canActivate: [AuthGuard] },
  { path: 'customers', component: CustomersComponent, canActivate: [AuthGuard] },
  { path: 'employees', component: EmployeesComponent, canActivate: [AuthGuard] },
  { path: 'reports', component: ReportsComponent, canActivate: [AuthGuard] },
  { path: 'manufacturer', component: ManufacturerComponent, canActivate: [AuthGuard] },
  { path: 'invoices', component: InvoiceComponent, canActivate: [AuthGuard] },
  { path: 'promotions', component: PromotionsComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '/login' }
];
