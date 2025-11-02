import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment'; 
export interface Product {
  productId?: number;
  productCode: string;
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
export class ProductService {
  private apiUrl = `${environment.apiUrl}/api/Product`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  getAllProducts(): Observable<ApiResponse<Product[]>> {
    console.log('📡 Getting all products from:', `${this.apiUrl}/GetAll`);
    return this.http.get<ApiResponse<Product[]>>(
      `${this.apiUrl}/GetAll`,
      { headers: this.getHeaders() }
    );
  }

  getProductById(id: number): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() }
    );
  }

  createProduct(product: Product): Observable<any> {
    console.log('📤 Creating product:', product);
    return this.http.post<any>(
      `${this.apiUrl}/Create`,
      product,
      { headers: this.getHeaders() }
    );
  }

  updateProduct(id: number, product: Product): Observable<any> {
    return this.http.put<any>(
      `${this.apiUrl}/Update/${id}`,
      product,
      { headers: this.getHeaders() }
    );
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete<any>(
      `${this.apiUrl}/Delete/${id}`,
      { headers: this.getHeaders() }
    );
  }
}
