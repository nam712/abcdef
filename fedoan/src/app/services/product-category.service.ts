import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

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
  private apiUrl = '${environment.apiUrl}/api/ProductCategory'; // Hardcode tạm để test

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  getAllCategories(): Observable<ApiResponse<ProductCategory[]>> {
    console.log('📡 Getting all categories from:', `${this.apiUrl}/GetAll`);
    return this.http.get<ApiResponse<ProductCategory[]>>(
      `${this.apiUrl}/GetAll`,
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
    return this.http.post<any>(
      `${this.apiUrl}/Create`,
      { categoryName: categoryName },
      { headers: this.getHeaders() }
    );
  }

  updateCategory(id: number, categoryName: string): Observable<any> {
    return this.http.put<any>(
      `${this.apiUrl}/Update/${id}`,
      { categoryName: categoryName },
      { headers: this.getHeaders() }
    );
  }

  deleteCategory(id: number): Observable<any> {
    return this.http.delete<any>(
      `${this.apiUrl}/Delete/${id}`,
      { headers: this.getHeaders() }
    );
  }
}
