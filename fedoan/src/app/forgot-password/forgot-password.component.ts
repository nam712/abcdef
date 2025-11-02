import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit {
  forgotPasswordForm!: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  showSuccessScreen = false;
  maskedPhone = '';

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.forgotPasswordForm = this.formBuilder.group({
      phone: ['', [
        Validators.required,
        Validators.pattern(/^0[0-9]{9}$/),
        Validators.minLength(10),
        Validators.maxLength(10)
      ]]
    });
  }

  onSubmit(): void {
    if (this.forgotPasswordForm.invalid) {
      this.markFormGroupTouched(this.forgotPasswordForm);
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.isLoading = true;

    const phone = this.forgotPasswordForm.value.phone.trim();

    console.log('📤 Gửi yêu cầu quên mật khẩu cho SĐT:', phone);

    this.authService.forgotPassword(phone).subscribe({
      next: (response) => {
        console.log('✅ Forgot password successful:', response);
        this.isLoading = false;
        
        if (response.success && response.data) {
          this.maskedPhone = response.data.phone || phone;
          this.showSuccessScreen = true;
          this.successMessage = response.message || 'Mật khẩu mới đã được gửi về số điện thoại của bạn';
        } else {
          this.errorMessage = response.message || 'Có lỗi xảy ra';
        }
      },
      error: (error) => {
        console.error('❌ Forgot password error:', error);
        this.isLoading = false;
        
        if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else if (error.error?.errors && error.error.errors.length > 0) {
          this.errorMessage = error.error.errors.join(', ');
        } else if (error.status === 0) {
          this.errorMessage = 'Không thể kết nối tới server. Vui lòng kiểm tra kết nối mạng.';
        } else {
          this.errorMessage = 'Có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.';
        }
      }
    });
  }

  backToLogin(): void {
    this.router.navigate(['/login']);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.forgotPasswordForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.forgotPasswordForm.get(fieldName);
    if (field?.hasError('required')) {
      return 'Vui lòng nhập số điện thoại';
    }
    if (field?.hasError('pattern') || field?.hasError('minlength') || field?.hasError('maxlength')) {
      return 'Số điện thoại không hợp lệ (10 chữ số, bắt đầu bằng 0)';
    }
    return '';
  }
}
