import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Employee } from '../employees/employees.component';
import { environment } from '../../environments/environment';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private apiUrl = `${environment.apiUrl}/api/employees`; // RESTful format - lowercase

  constructor(private http: HttpClient) {
    console.log('🔧 Employee Service initialized');
    console.log('📡 API URL:', this.apiUrl);
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token'); // ✅ Đổi thành auth_token
    console.log('🔑 Employee Token:', token ? 'Available' : 'Not found');
    
    if (!token) {
      console.warn('⚠️ No authentication token found');
    }
    
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  getAllEmployees(): Observable<ApiResponse<Employee[]>> {
    // RESTful format - NO /GetAll
    return this.http.get<ApiResponse<Employee[]>>(this.apiUrl, {
      headers: this.getHeaders()
    });
  }

  getEmployeeById(id: number): Observable<ApiResponse<Employee>> {
    return this.http.get<ApiResponse<Employee>>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  searchEmployees(name: string): Observable<ApiResponse<Employee[]>> {
    return this.http.get<ApiResponse<Employee[]>>(`${this.apiUrl}/search?name=${encodeURIComponent(name)}`, {
      headers: this.getHeaders()
    });
  }

  createEmployee(employee: any): Observable<ApiResponse<any>> {
    console.log('📤 Creating employee:', employee);
    // RESTful format - NO /Create
    return this.http.post<ApiResponse<any>>(this.apiUrl, employee, {
      headers: this.getHeaders()
    });
  }

  updateEmployee(id: number, employee: any): Observable<ApiResponse<any>> {
    console.log('📤 PUT request to:', `${this.apiUrl}/${id}`);
    console.log('📤 Payload:', employee);
    // RESTful format - NO /Update
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}`, employee, {
      headers: this.getHeaders()
    });
  }

  deleteEmployee(id: number): Observable<ApiResponse<any>> {
    // RESTful format - NO /Delete
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  // New: Get profile for an employee (used by Employee to fetch their own profile)
  getProfile(id: number): Observable<ApiResponse<Employee>> {
    return this.http.get<ApiResponse<Employee>>(`${this.apiUrl}/${id}/profile`, {
      headers: this.getHeaders()
    });
  }

  // New: Employee updates own profile (uses dedicated endpoint)
  updateProfile(id: number, dto: any): Observable<ApiResponse<any>> {
    console.log('📤 PUT (update-profile) to:', `${this.apiUrl}/${id}/update-profile`);
    console.log('📤 Payload (profile update):', dto);
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}/update-profile`, dto, {
      headers: this.getHeaders()
    });
  }
}
