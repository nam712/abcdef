import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Customer {
  customerId?: number;
  customerCode?: string;
  customerName: string;
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
  status?: string;
  notes?: string;
  createdDate?: string;
  updatedDate?: string;
  contactPerson?: string;
  shop_owner_id?: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private apiUrl = `${environment.apiUrl}/api/customers`;

  constructor(private http: HttpClient) {
    console.log('🔧 Customer Service initialized');
    console.log('📡 API URL:', this.apiUrl);
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token') || localStorage.getItem('access_token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` })
    });
  }

  getAllCustomers(): Observable<ApiResponse<Customer[]>> {
    return this.http.get<ApiResponse<Customer[]>>(this.apiUrl, {
      headers: this.getHeaders()
    });
  }

  getCustomerById(id: number): Observable<ApiResponse<Customer>> {
    return this.http.get<ApiResponse<Customer>>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  searchCustomers(query: string): Observable<ApiResponse<Customer[]>> {
    return this.http.get<ApiResponse<Customer[]>>(`${this.apiUrl}/search?q=${query}`, {
      headers: this.getHeaders()
    });
  }

  createCustomer(customer: any): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(this.apiUrl, customer, {
      headers: this.getHeaders()
    });
  }

  updateCustomer(id: number, customer: any): Observable<ApiResponse<any>> {
    console.log('📤 PUT request to:', `${this.apiUrl}/${id}`);
    console.log('📤 Payload:', customer);
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}`, customer, {
      headers: this.getHeaders()
    });
  }

  deleteCustomer(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }
}
