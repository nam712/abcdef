import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Supplier {
  supplierCode: string;
  supplierName: string;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
  taxCode?: string | null;
  bankAccount?: string;
  bankName?: string;
  priceList?: string;
  logoUrl?: string | null;
  status: string;
  notes?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T | any; // Cho phép any để xử lý các format khác nhau
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private apiUrl = `${environment.apiUrl}/api/Supplier`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    console.log('🔑 Token:', token ? 'Available' : 'Not found');
    
    if (!token) {
      console.warn('⚠️ No authentication token found');
    }
    
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  getAllSuppliers(page: number = 1, pageSize: number = 10): Observable<ApiResponse<Supplier>> {
    console.log('📡 Calling API:', `${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
    return this.http.get<ApiResponse<Supplier>>(
      `${this.apiUrl}?page=${page}&pageSize=${pageSize}`,
      { headers: this.getHeaders() }
    );
  }

  getSupplierById(id: number): Observable<ApiResponse<Supplier>> {
    return this.http.get<ApiResponse<Supplier>>(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() }
    );
  }

  createSupplier(supplier: Supplier): Observable<ApiResponse<Supplier>> {
    console.log('📤 Creating supplier:', supplier);
    console.log('🌐 Full API URL:', this.apiUrl);
    console.log('🌍 Environment API URL:', environment.apiUrl);
    
    return this.http.post<ApiResponse<Supplier>>(
      this.apiUrl,
      supplier,
      { headers: this.getHeaders() }
    );
  }

  updateSupplier(id: number, supplier: Supplier): Observable<ApiResponse<Supplier>> {
    return this.http.put<ApiResponse<Supplier>>(
      `${this.apiUrl}/${id}`,
      supplier,
      { headers: this.getHeaders() }
    );
  }

  deleteSupplier(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() }
    );
  }

  searchSuppliers(searchDto: any): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(
      `${this.apiUrl}/search`,
      { 
        params: searchDto,
        headers: this.getHeaders() 
      }
    );
  }
}
