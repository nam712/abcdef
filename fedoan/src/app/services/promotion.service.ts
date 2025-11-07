import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface Promotion {
  promotionId?: number;
  promotionCode: string;
  promotionName: string;
  description?: string;
  promotionType: 'percentage' | 'fixed' | 'buy_x_get_y' | 'free_shipping';
  discountValue: number;
  minPurchaseAmount?: number;
  maxDiscountAmount?: number;
  startDate: string;
  endDate: string;
  status: 'active' | 'inactive' | 'expired';
  usageLimit?: number;
  usageCount: number;
  applicableProducts?: number[];
  applicableCustomers?: number[];
  shopOwnerId?: number;
  createdAt?: string;
  updatedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PromotionService {
  private apiUrl = 'http://localhost:5001/api/Promotions';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  getAllPromotions(): Observable<any> {
    return this.http.get<any>(this.apiUrl, { headers: this.getHeaders() });
  }

  getPromotionById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  createPromotion(promotion: Promotion): Observable<any> {
    return this.http.post<any>(this.apiUrl, promotion, { headers: this.getHeaders() });
  }

  updatePromotion(id: number, promotion: Promotion): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, promotion, { headers: this.getHeaders() });
  }

  deletePromotion(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  validatePromotion(promotionCode: string, totalAmount: number, customerId?: number): Observable<any> {
    const payload: any = {
      promotionCode,
      totalAmount
    };
    
    if (customerId) {
      payload.customerId = customerId;
    }
    
    return this.http.post<any>(`${this.apiUrl}/validate`, payload, { headers: this.getHeaders() });
  }

  applyPromotion(promotionId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/apply/${promotionId}`, {}, { headers: this.getHeaders() });
  }
}
