import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, interval } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

export interface Notification {
  id: number;
  message: string;
  type: 'info' | 'warning' | 'success' | 'error';
  time: string;
  isRead: boolean;
  timestamp: Date;
  route?: string; // URL để navigate khi click
  entityType?: string;
  entityId?: number;
}

interface BackendNotification {
  notificationId: number;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  readAt: string | null;
  userId: number | null;
  entityType: string | null;
  entityId: number | null;
  action: string | null;
  metadata: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/api/Notification`;
  private notifications$ = new BehaviorSubject<Notification[]>([]);
  private unreadCount$ = new BehaviorSubject<number>(0);
  private useBackend = true;
  private pollingInterval = 30000;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {
    this.loadNotifications();
    
    if (this.useBackend) {
      this.startPolling();
    }
  }

  // ✅ Thêm method getHeaders để dùng auth_token
  private getHeaders() {
    const token = localStorage.getItem('auth_token');
    return {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    };
  }

  getNotifications(): Observable<Notification[]> {
    return this.notifications$.asObservable();
  }

  getUnreadCount(): Observable<number> {
    return this.unreadCount$.asObservable();
  }

  addNotification(
    message: string, 
    type: 'info' | 'warning' | 'success' | 'error' = 'info',
    options?: {
      entityType?: string;
      entityId?: string | number;
      action?: string;
      metadata?: any;
      route?: string; // Thêm route để navigate
    }
  ): void {
    if (this.useBackend) {
      const userId = this.authService.getCurrentUserId();
      
      const payload = {
        message,
        type,
        userId: userId,
        entityType: options?.entityType || null,
        entityId: options?.entityId?.toString() || null,
        action: options?.action || null,
        metadata: options?.metadata ? JSON.stringify(options.metadata) : null
      };

      console.log('📤 Sending notification to backend:', payload);

      // ✅ Dùng getHeaders()
      this.http.post<any>(this.apiUrl, payload, { headers: this.getHeaders() }).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('✅ Notification created on backend');
            this.loadNotificationsFromBackend();
          }
        },
        error: (error) => {
          console.error('❌ Error creating notification on backend:', error);
          this.addNotificationLocal(message, type);
        }
      });
    } else {
      this.addNotificationLocal(
        message, 
        type, 
        options?.route,
        options?.entityType,
        typeof options?.entityId === 'number' ? options.entityId : undefined
      );
    }
  }

  private addNotificationLocal(
    message: string, 
    type: 'info' | 'warning' | 'success' | 'error',
    route?: string,
    entityType?: string,
    entityId?: number
  ): void {
    const notification: Notification = {
      id: Date.now(),
      message,
      type,
      time: this.getRelativeTime(new Date()),
      isRead: false,
      timestamp: new Date(),
      route: route,
      entityType: entityType,
      entityId: entityId
    };

    const currentNotifications = this.notifications$.value;
    const updatedNotifications = [notification, ...currentNotifications];

    // Giới hạn số lượng thông báo
    if (updatedNotifications.length > 50) {
      updatedNotifications.splice(50);
    }

    this.notifications$.next(updatedNotifications);
    this.updateUnreadCount();
    this.saveNotificationsToStorage();
  }

  markAsRead(notificationId: number): void {
    if (this.useBackend) {
      // ✅ Dùng getHeaders()
      this.http.patch(`${this.apiUrl}/${notificationId}/read`, {}, { headers: this.getHeaders() }).subscribe({
        next: () => {
          console.log('✅ Marked as read on backend');
          this.loadNotificationsFromBackend();
        },
        error: (error) => {
          console.error('❌ Error marking as read:', error);
          this.markAsReadLocal(notificationId);
        }
      });
    } else {
      this.markAsReadLocal(notificationId);
    }
  }

  private markAsReadLocal(notificationId: number): void {
    const notifications = this.notifications$.value;
    const notification = notifications.find(n => n.id === notificationId);
    
    if (notification && !notification.isRead) {
      notification.isRead = true;
      this.notifications$.next([...notifications]);
      this.updateUnreadCount();
      this.saveNotificationsToStorage();
    }
  }

  markAllAsRead(): void {
    if (this.useBackend) {
      // ✅ Dùng getHeaders()
      this.http.patch(`${this.apiUrl}/read-all`, {}, { headers: this.getHeaders() }).subscribe({
        next: () => {
          console.log('✅ Marked all as read on backend');
          this.loadNotificationsFromBackend();
        },
        error: (error) => {
          console.error('❌ Error marking all as read:', error);
          this.markAllAsReadLocal();
        }
      });
    } else {
      this.markAllAsReadLocal();
    }
  }

  private markAllAsReadLocal(): void {
    const notifications = this.notifications$.value.map(n => ({
      ...n,
      isRead: true
    }));
    
    this.notifications$.next(notifications);
    this.updateUnreadCount();
    this.saveNotificationsToStorage();
  }

  clearAll(): void {
    if (this.useBackend) {
      // ✅ Dùng getHeaders()
      this.http.delete(`${this.apiUrl}/all`, { headers: this.getHeaders() }).subscribe({
        next: () => {
          console.log('✅ Cleared all on backend');
          this.loadNotificationsFromBackend();
        },
        error: (error) => {
          console.error('❌ Error clearing all:', error);
          this.clearAllLocal();
        }
      });
    } else {
      this.clearAllLocal();
    }
  }

  private clearAllLocal(): void {
    this.notifications$.next([]);
    this.unreadCount$.next(0);
    this.saveNotificationsToStorage();
  }

  private updateUnreadCount(): void {
    const unreadCount = this.notifications$.value.filter(n => !n.isRead).length;
    this.unreadCount$.next(unreadCount);
  }

  private getRelativeTime(date: Date): string {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Vừa xong';
    if (diffMins < 60) return `${diffMins} phút trước`;
    if (diffHours < 24) return `${diffHours} giờ trước`;
    if (diffDays < 7) return `${diffDays} ngày trước`;
    return date.toLocaleDateString('vi-VN');
  }

  private scheduleTimeUpdate(): void {
    // Cập nhật thời gian hiển thị mỗi phút
    setTimeout(() => {
      const notifications = this.notifications$.value.map(n => ({
        ...n,
        time: this.getRelativeTime(n.timestamp)
      }));
      this.notifications$.next(notifications);
    }, 60000);
  }

  private saveNotificationsToStorage(): void {
    try {
      const notifications = this.notifications$.value;
      localStorage.setItem('app-notifications', JSON.stringify(notifications));
    } catch (error) {
      console.error('Error saving notifications to storage:', error);
    }
  }

  private loadNotificationsFromStorage(): void {
    try {
      const stored = localStorage.getItem('app-notifications');
      if (stored) {
        const notifications: Notification[] = JSON.parse(stored);
        // Cập nhật lại thời gian hiển thị
        const updatedNotifications = notifications.map(n => ({
          ...n,
          timestamp: new Date(n.timestamp),
          time: this.getRelativeTime(new Date(n.timestamp))
        }));
        this.notifications$.next(updatedNotifications);
        this.updateUnreadCount();
      }
    } catch (error) {
      console.error('Error loading notifications from storage:', error);
    }
  }

  // Load notifications - chọn backend hoặc localStorage
  private loadNotifications(): void {
    if (this.useBackend) {
      this.loadNotificationsFromBackend();
    } else {
      this.loadNotificationsFromStorage();
    }
  }

  // Load từ backend
  private loadNotificationsFromBackend(): void {
    // ✅ Dùng getHeaders()
    this.http.get<any>(`${this.apiUrl}?limit=50`, { headers: this.getHeaders() }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          console.log('✅ Loaded notifications from backend:', response.data.length);
          const notifications = response.data.map((n: BackendNotification) => this.mapBackendToFrontend(n));
          this.notifications$.next(notifications);
          this.updateUnreadCount();
        }
      },
      error: (error) => {
        console.error('❌ Error loading from backend:', error);
        this.useBackend = false;
        this.loadNotificationsFromStorage();
      }
    });
  }

  // Map backend notification to frontend format
  private mapBackendToFrontend(backendNotif: BackendNotification): Notification {
    const timestamp = new Date(backendNotif.createdAt);
    
    // Tạo route dựa trên entityType và entityId
    let route: string | undefined = undefined;
    if (backendNotif.entityType && backendNotif.entityId) {
      route = this.generateRoute(backendNotif.entityType, backendNotif.entityId);
    }
    
    return {
      id: backendNotif.notificationId,
      message: backendNotif.message,
      type: backendNotif.type as 'info' | 'warning' | 'success' | 'error',
      time: this.getRelativeTime(timestamp),
      isRead: backendNotif.isRead,
      timestamp: timestamp,
      route: route,
      entityType: backendNotif.entityType || undefined,
      entityId: backendNotif.entityId || undefined
    };
  }

  // Tạo route URL từ entityType và entityId
  private generateRoute(entityType: string, entityId: number | string): string {
    const routes: { [key: string]: string } = {
      'Product': '/products',
      'Customer': '/customers',
      'Employee': '/employees',
      'Supplier': '/manufacturer',
      'Invoice': '/pos',
      'PurchaseOrder': '/purchase-orders'
    };
    
    return routes[entityType] || '/dashboard';
  }

  // Polling để cập nhật thông báo định kỳ
  private startPolling(): void {
    interval(this.pollingInterval).subscribe(() => {
      this.loadNotificationsFromBackend();
    });
  }

  // Get stats từ backend
  getStats(): Observable<any> {
    // ✅ Dùng getHeaders()
    return this.http.get(`${this.apiUrl}/stats`, { headers: this.getHeaders() });
  }
}
