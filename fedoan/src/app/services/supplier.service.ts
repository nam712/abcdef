import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
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
  priceList?: string | number;
  logoUrl?: string | null;
  status: string;
  notes?: string;
  shopOwnerId?: string; // thêm optional, sẽ lấy từ token
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T | any;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private apiUrl = `${environment.apiUrl}/api/Supplier`;

  constructor(private http: HttpClient) {
    console.log('🔧 Supplier Service initialized');
    console.log('📡 API URL:', this.apiUrl);
  }

  // ----- JWT & Headers -----
  private getToken(): string | null {
    return localStorage.getItem('token');
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token'); // ✅ Thêm auth_token
    console.log('🔑 Supplier Token:', token ? 'Available' : 'Not found');
    
    if (!token) {
      console.warn('⚠️ No authentication token found');
    }
    
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  private parseJwt(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      return JSON.parse(atob(base64));
    } catch (error) {
      console.error('❌ Failed to parse JWT', error);
      return null;
    }
  }

  private getShopOwnerId(): string | null {
    const token = this.getToken();
    if (!token) return null;
    const payload = this.parseJwt(token);
    return payload?.sub || null; // Hoặc payload.shop_owner_id nếu claim khác
  }

  // ----- CRUD -----
  getAllSuppliers(page: number = 1, pageSize: number = 10): Observable<ApiResponse<Supplier[]>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    console.log('📡 GET:', this.apiUrl);
    return this.http.get<ApiResponse<Supplier[]>>(this.apiUrl, {
      headers: this.getHeaders(),
      params
    });
  }

  getSupplierById(id: number): Observable<ApiResponse<Supplier>> {
    return this.http.get<ApiResponse<Supplier>>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  createSupplier(supplier: Supplier): Observable<ApiResponse<Supplier>> {
    const shopOwnerId = this.getShopOwnerId();
    if (shopOwnerId) {
      supplier.shopOwnerId = shopOwnerId; // gán tự động
    }
    console.log('📡 POST:', this.apiUrl);
    console.log('📦 Sending data:', JSON.stringify(supplier, null, 2));
    
    return this.http.post<ApiResponse<Supplier>>(this.apiUrl, supplier, {
      headers: this.getHeaders()
    });
  }

  updateSupplier(id: number, supplier: Supplier): Observable<ApiResponse<Supplier>> {
    const shopOwnerId = this.getShopOwnerId();
    if (shopOwnerId) {
      supplier.shopOwnerId = shopOwnerId; // gán tự động
    }
    const url = `${this.apiUrl}/${id}`;
    console.log('📡 PUT:', url);
    console.log('📦 Sending data:', JSON.stringify(supplier, null, 2));
    
    return this.http.put<ApiResponse<Supplier>>(url, supplier, {
      headers: this.getHeaders()
    });
  }

  deleteSupplier(id: number): Observable<ApiResponse<any>> {
    const url = `${this.apiUrl}/${id}`;
    console.log('📡 DELETE:', url);
    
    return this.http.delete<ApiResponse<any>>(url, {
      headers: this.getHeaders()
    });
  }

  searchSuppliers(searchDto: any): Observable<ApiResponse<Supplier[]>> {
    let params = new HttpParams();
    for (const key in searchDto) {
      if (searchDto[key] != null) {
        params = params.set(key, searchDto[key]);
      }
    }
    return this.http.get<ApiResponse<Supplier[]>>(`${this.apiUrl}/search`, {
      headers: this.getHeaders(),
      params
    });
  }
}
