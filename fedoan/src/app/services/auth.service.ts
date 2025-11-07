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

interface TokenPayload {
  shop_owner_id: string;
  user_id: string;
  email: string;
  exp: number;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/api/Auth`; // Đổi 'auth' thành 'Auth'
  private readonly TOKEN_KEY = 'access_token';

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
    localStorage.setItem(this.TOKEN_KEY, token);
    console.log('✅ Token saved to localStorage');
  }

  getToken(): string | null {
    return localStorage.getItem('auth_token') || localStorage.getItem(this.TOKEN_KEY);
  }

  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem('auth_token');
  }

  decodeToken(): TokenPayload | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  getShopOwnerId(): string | null {
    const decoded = this.decodeToken();
    console.log('🔍 Getting shop_owner_id from token:', decoded);
    
    if (!decoded) return null;
    
    const decodedAny = decoded as any;
    const shopOwnerId = decodedAny.shop_owner_id 
                     || decodedAny.shopOwnerId 
                     || decodedAny.ShopOwnerId
                     || null;
    
    console.log('🏪 Resolved shop_owner_id:', shopOwnerId);
    return shopOwnerId;
  }

  getCurrentUserId(): string | null {
    const decoded = this.decodeToken();
    
    if (!decoded) return null;
    
    const decodedAny = decoded as any;
    const userId = decodedAny.user_id 
                || decodedAny.userId 
                || decodedAny.UserId
                || null;
    
    return userId;
  }

  /**
   * Get the user type/role from the decoded token (e.g. 'ShopOwner' or 'Employee')
   */
  getUserType(): string | null {
    const decoded = this.decodeToken();
    if (!decoded) return null;
    const d: any = decoded as any;
    return d.user_type || d.userType || d.role || null;
  }

  isTokenExpired(): boolean {
    const decoded = this.decodeToken();
    if (!decoded) return true;

    const currentTime = Date.now() / 1000;
    return decoded.exp < currentTime;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return token !== null && !this.isTokenExpired();
  }

  logout(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem('user_data');
    console.log('🚪 Logged out - Token removed from localStorage');
  }

  forgotPassword(phone: string): Observable<AuthResponse> {
    const forgotPasswordData = { phone };
    console.log('📤 Sending forgot password request:', forgotPasswordData);
    return this.http.post<AuthResponse>(`${this.apiUrl}/forgot-password`, forgotPasswordData);
  }
}