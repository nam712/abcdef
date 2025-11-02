import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  console.log('🔐 Auth Guard checking...', state.url);
  
  if (authService.isAuthenticated()) {
    console.log('✅ User authenticated, allowing access to:', state.url);
    return true;
  }
  
  console.log('❌ User not authenticated, redirecting to login');
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
