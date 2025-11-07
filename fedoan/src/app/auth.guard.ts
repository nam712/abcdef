import { Injectable } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard {
  constructor(private router: Router) {}

  canActivate(): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    const token = localStorage.getItem('auth_token') || localStorage.getItem('access_token');
    
    if (token) {
      // Token exists, allow access
      return true;
    } else {
      // No token, redirect to login
      console.warn('⚠️ No auth token found, redirecting to login');
      return this.router.createUrlTree(['/login']);
    }
  }
}
