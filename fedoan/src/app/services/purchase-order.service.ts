import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T | any;
  errors?: string[];
}

export interface CreatePurchaseOrderDto {
  poCode?: string;
  supplierId: number;
  poDate?: string; // ISO date
  expectedDeliveryDate?: string; // ISO date
  notes?: string;
  details: Array<{ productId: number; quantity: number; importPrice: number }>;
}

@Injectable({ providedIn: 'root' })
export class PurchaseOrderService {
  private apiUrl = `${environment.apiUrl}/api/PurchaseOrder`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token'); // ✅ Thêm auth_token
    console.log('🔑 Purchase Order Token:', token ? 'Available' : 'Not found');
    
    if (!token) {
      console.warn('⚠️ No authentication token found');
    }
    
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  getAll(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(this.apiUrl, { headers: this.getHeaders() });
  }

  getById(id: number): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  create(dto: CreatePurchaseOrderDto): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(this.apiUrl, dto, { headers: this.getHeaders() });
  }

  delete(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }
}
