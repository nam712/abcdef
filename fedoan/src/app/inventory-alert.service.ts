import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class InventoryAlertService {

  constructor(private http: HttpClient) { }

  checkInventory() {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    this.http.get<any[]>('http://localhost:5001/api/Product/GetAll', { headers })
      .subscribe({
        next: (products) => {
          // ...existing code...
        },
        error: (error) => {
          console.error('Error checking inventory:', error);
          // Không hiển thị alert nếu chưa đăng nhập
          if (error.status !== 401) {
            console.error('Inventory check failed:', error);
          }
        }
      });
  }
}