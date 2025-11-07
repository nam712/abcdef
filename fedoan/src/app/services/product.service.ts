import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Product {
  productId?: number;
  productCode?: string;
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
export class ProductService {
  private apiUrl = `${environment.apiUrl}/api/Product`;

  constructor(private http: HttpClient) {
    console.log('🔧 Product Service initialized');
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

  getAllProducts(): Observable<ApiResponse<Product[]>> {
    console.log('📡 Getting all products from:', `${this.apiUrl}/GetAll`);
    return this.http.get<ApiResponse<Product[]>>(`${this.apiUrl}/GetAll`, {
      headers: this.getHeaders()
    });
  }

  getProductById(id: number): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  searchProducts(query: string): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(`${this.apiUrl}/search?q=${query}`, {
      headers: this.getHeaders()
    });
  }

  createProduct(product: any): Observable<ApiResponse<any>> {
    console.log('📤 Creating product:', product);
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/Create`, product, {
      headers: this.getHeaders()
    });
  }

  updateProduct(id: number, product: any): Observable<ApiResponse<any>> {
    console.log('📤 PUT request to:', `${this.apiUrl}/Update/${id}`);
    console.log('📤 Payload:', product);
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/Update/${id}`, product, {
      headers: this.getHeaders()
    });
  }

  deleteProduct(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/Delete/${id}`, {
      headers: this.getHeaders()
    });
  }
}
