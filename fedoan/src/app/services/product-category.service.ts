import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment'; // ✅ Import environment

export interface ProductCategory {
  categoryId?: number;
  categoryName: string;
  description?: string;
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
export class ProductCategoryService {
  private apiUrl = `${environment.apiUrl}/api/ProductCategory`; // ✅ Fix template literal

  constructor(private http: HttpClient) {
    console.log('🔧 Product Category Service initialized');
    console.log('📡 API URL:', this.apiUrl);
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token'); // ✅ Đổi từ 'token' sang 'auth_token'
    console.log('🔑 Product Category Token:', token ? 'Available' : 'Not found');
    
    if (!token) {
      console.warn('⚠️ No authentication token found');
    }
    
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  getAllCategories(): Observable<ApiResponse<ProductCategory[]>> {
    console.log('📡 Getting all categories from:', this.apiUrl);
    // ✅ Try RESTful first (no /GetAll)
    return this.http.get<ApiResponse<ProductCategory[]>>(
      this.apiUrl,
      { headers: this.getHeaders() }
    );
  }

  getCategoryById(id: number): Observable<ApiResponse<ProductCategory>> {
    return this.http.get<ApiResponse<ProductCategory>>(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() }
    );
  }

  createCategory(categoryName: string): Observable<any> {
    console.log('📤 Creating category:', categoryName);
    // ✅ Try RESTful POST (no /Create)
    return this.http.post<any>(
      this.apiUrl,
      { categoryName: categoryName },
      { headers: this.getHeaders() }
    );
  }

  updateCategory(id: number, categoryName: string): Observable<any> {
    console.log('📤 Updating category:', id, categoryName);
    // ✅ Try RESTful PUT /{id} (no /Update)
    return this.http.put<any>(
      `${this.apiUrl}/${id}`,
      { categoryName: categoryName },
      { headers: this.getHeaders() }
    );
  }

  deleteCategory(id: number): Observable<any> {
    console.log('📤 Deleting category:', id);
    // ✅ Try RESTful DELETE /{id} (no /Delete)
    return this.http.delete<any>(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() }
    );
  }
}
