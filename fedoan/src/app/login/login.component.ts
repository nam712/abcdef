import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  showPassword = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.formBuilder.group({
      phone: ['', [Validators.required, Validators.pattern(/^[0-9]{9,11}$/)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    // Kiểm tra nếu đã đăng nhập thì chuyển trang
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched(this.loginForm);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const phone = this.loginForm.value.phone.trim();
    const password = this.loginForm.value.password;

    console.log('📤 Logging in with phone:', phone);

    this.authService.login(phone, password).subscribe({
      next: (response) => {
        this.isLoading = false;
        console.log('✅ Login successful:', response);
        
        if (response.success && response.data?.token) {
          // Lưu token vào localStorage
          this.authService.saveToken(response.data.token);
          
          this.successMessage = 'Đăng nhập thành công!';
          
          // Chuyển hướng sau 1 giây
          setTimeout(() => {
            this.router.navigate(['/dashboard']);
          }, 1000);
        } else {
          this.errorMessage = response.message || 'Đăng nhập thất bại';
        }
      },
      error: (error) => {
        this.isLoading = false;
        console.error('❌ Login error:', error);
        
        if (error.status === 401) {
          this.errorMessage = 'Số điện thoại hoặc mật khẩu không đúng';
        } else if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage = 'Có lỗi xảy ra. Vui lòng thử lại sau.';
        }
      }
    });
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return 'Trường này là bắt buộc';
      if (field.errors['minlength']) return `Tối thiểu ${field.errors['minlength'].requiredLength} ký tự`;
      if (field.errors['pattern']) return 'Số điện thoại không hợp lệ';
    }
    return '';
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}

