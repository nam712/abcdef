import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Customer {
  customerId?: number;
  customerCode: string;
  customerName: string;
  contactPerson?: string; // <-- Thêm dòng này
  phone: string;
  email?: string;
  address?: string;
  taxCode?: string;
  customerType?: string;
  dateOfBirth?: string;
  gender?: string;
  idCard?: string;
  bankAccount?: string;
  bankName?: string;
  totalDebt?: number;
  totalPurchaseAmount?: number;
  totalPurchaseCount?: number;
  loyaltyPoints?: number;
  segment?: string;
  source?: string;
  avatarUrl?: string;
  status: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  // Sửa lại đường dẫn API, không dùng template string nếu chưa import environment
  private apiUrl = `${environment.apiUrl}/api/customers`;
  // Nếu bạn đã có environment, hãy import và dùng: import { environment } from '../../environments/environment';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  getAllCustomers(): Observable<Customer[]> {
    return this.http.get<Customer[]>(this.apiUrl, { headers: this.getHeaders() });
  }

  getCustomerById(id: number): Observable<Customer> {
    return this.http.get<Customer>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  createCustomer(customer: Customer): Observable<Customer> {
    // Chỉ gửi các trường backend cho phép (ví dụ: bỏ createdAt, updatedAt nếu không cần)
    const payload: any = { ...customer };
    delete payload.createdAt;
    delete payload.updatedAt;
    // Nếu backend không nhận segment, source, avatarUrl... cũng xóa tương tự

    return this.http.post<Customer>(this.apiUrl, payload, { headers: this.getHeaders() });
  }

  updateCustomer(id: number, customer: Customer): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, customer, { headers: this.getHeaders() });
  }

  deleteCustomer(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  searchCustomers(name: string): Observable<Customer[]> {
    return this.http.get<Customer[]>(`${this.apiUrl}/search?name=${encodeURIComponent(name)}`, { headers: this.getHeaders() });
  }
}
