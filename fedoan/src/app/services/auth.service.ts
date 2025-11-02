import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface RegisterRequest {
  shopOwnerName: string;
  phone: string;
  email?: string | null;
  address?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  taxCode?: string | null;
  businessCategoryId?: number | null;
  shopName: string;
  shopAddress?: string | null;
  shopDescription?: string | null;
  password: string;
  confirmPassword: string;
  termsAndConditionsAgreed: boolean;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  data?: any;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Hardcode tạm thời để test
  private apiUrl = `${environment.apiUrl}/api/auth` ;

  constructor(private http: HttpClient) {
    console.log('🔧 Auth Service initialized');
    console.log('📡 API URL:', this.apiUrl);
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    const url = `${this.apiUrl}/register`;
    console.log('📤 Sending registration request to:', url);
    console.log('📦 Registration data:', JSON.stringify(data, null, 2));
    return this.http.post<AuthResponse>(url, data);
  }

  login(phone: string, password: string): Observable<AuthResponse> {
    const loginData = { phone, password };
    console.log('📤 Sending login request:', loginData);
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, loginData);
  }

  saveToken(token: string): void {
    localStorage.setItem('auth_token', token);
    console.log('✅ Token saved to localStorage');
  }

  getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  getCurrentUserId(): string | null {
    // Lấy user data từ localStorage
    const userData = localStorage.getItem('user_data');
    if (userData) {
      try {
        const user = JSON.parse(userData);
        return user.employeeId || user.shopOwnerId || user.id || null;
      } catch (e) {
        console.error('Error parsing user data:', e);
      }
    }
    
    // Hoặc decode từ JWT token
    const token = this.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        return payload.sub || payload.userId || payload.nameid || null;
      } catch (e) {
        console.error('Error decoding token:', e);
      }
    }
    
    return null;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return !!token;
  }

  logout(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user_data');
    console.log('🚪 Logged out - Token removed from localStorage');
  }

  forgotPassword(phone: string): Observable<AuthResponse> {
    const forgotPasswordData = { phone };
    console.log('📤 Sending forgot password request:', forgotPasswordData);
    return this.http.post<AuthResponse>(`${this.apiUrl}/forgot-password`, forgotPasswordData);
  }
}