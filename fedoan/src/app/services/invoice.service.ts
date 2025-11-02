import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Invoice, ApiResponse } from '../models/invoice.model';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  // Backend controller is named InvoicesController -> route is /api/Invoices
  private apiUrl = `${environment.apiUrl}/api/Invoices`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    console.log('🔑 Token:', token ? 'Available' : 'Not found');
    
    if (!token) {
      console.warn('⚠️ No authentication token found');
    }
    
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  getAllInvoices(page: number = 1, pageSize: number = 10): Observable<ApiResponse<Invoice>> {
    console.log('📡 Calling API:', `${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
    return this.http.get<ApiResponse<Invoice>>(
      `${this.apiUrl}?page=${page}&pageSize=${pageSize}`,
      { headers: this.getHeaders() }
    );
  }

  getInvoiceById(id: number): Observable<ApiResponse<Invoice>> {
    return this.http.get<ApiResponse<Invoice>>(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() }
    );
  }

  createInvoice(invoice: Invoice): Observable<ApiResponse<Invoice>> {
    console.log('📤 Creating invoice:', invoice);
    return this.http.post<ApiResponse<Invoice>>(
      this.apiUrl,
      invoice,
      { headers: this.getHeaders() }
    );
  }

  updateInvoice(id: number, invoice: Invoice): Observable<ApiResponse<Invoice>> {
    return this.http.put<ApiResponse<Invoice>>(
      `${this.apiUrl}/${id}`,
      invoice,
      { headers: this.getHeaders() }
    );
  }

  deleteInvoice(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() }
    );
  }

  searchInvoices(searchDto: any): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(
      `${this.apiUrl}/search`,
      { 
        params: searchDto,
        headers: this.getHeaders() 
      }
    );
  }
}
