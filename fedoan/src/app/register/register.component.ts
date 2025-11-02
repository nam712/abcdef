import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService, RegisterRequest } from '../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  isLoading = false;
  generatedVerificationCode = '';
  errorMessage = '';
  successMessage = '';
  currentStep = 1;
  totalSteps = 3;

  countries = [
    { code: 'VN', name: 'Việt Nam', phoneCode: '+84', regions: [
      { code: 'HN', name: 'Hà Nội' },
      { code: 'HCM', name: 'TP. Hồ Chí Minh' },
      { code: 'DN', name: 'Đà Nẵng' }
    ]},
    { code: 'US', name: 'United States', phoneCode: '+1', regions: [] }
  ];

  businessCategories = [
    { id: 1, name: 'Thời trang & Phụ kiện' },
    { id: 2, name: 'Tạp hóa & Siêu thị mini' },
    { id: 3, name: 'Điện tử & Điện máy' },
    { id: 4, name: 'Nhà thuốc & Thiết bị y tế' },
    { id: 5, name: 'Mỹ phẩm & Hóa mỹ phẩm' },
    { id: 6, name: 'Cửa hàng Mẹ & Bé' },
    { id: 7, name: 'Đồ gia dụng & Đời sống' },
    { id: 8, name: 'Nội thất & Trang trí' },
    { id: 9, name: 'Văn phòng phẩm & Nhà sách' },
    { id: 10, name: 'Hoa & Quà tặng' },
    { id: 11, name: 'Vật liệu xây dựng' },
    { id: 12, name: 'Phụ tùng & Linh kiện' },
    { id: 13, name: 'Cửa hàng thú cưng' },
    { id: 14, name: 'Đồ thể thao & Dã ngoại' },
    { id: 15, name: 'Trang sức & Đồng hồ' }
  ];

  genders = [
    { value: 'Male', label: 'Nam' },
    { value: 'Female', label: 'Nữ' },
    { value: 'Other', label: 'Khác' }
  ];

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.formBuilder.group({
      // Bước 1: Thông tin cá nhân
      shopOwnerName: ['', [Validators.required, Validators.minLength(3)]],
      phone: ['', [Validators.required, Validators.pattern(/^[0-9]{9,11}$/)]],
      shopName: ['', [Validators.required, Validators.minLength(3)]],
      
      // Bước 2: Thông tin cửa hàng
      shopAddress: [''],
      businessCategoryId: [''],
      verificationCode: ['', [Validators.required, Validators.minLength(6)]],
      
      // Bước 3: Bảo mật
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      agreeToTerms: [false, Validators.requiredTrue]
    });

    this.generateVerificationCode();
  }

  ngOnInit(): void {
    // Không cần setupCountryWatcher nữa
  }

  private generateVerificationCode(): void {
    this.generatedVerificationCode = Math.random().toString(36).substring(2, 8).toUpperCase();
  }

  changeVerificationCode(): void {
    this.generateVerificationCode();
  }

  nextStep(): void {
    // Validate current step fields
    if (!this.isCurrentStepValid()) {
      this.markCurrentStepTouched();
      return;
    }

    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
      this.errorMessage = '';
    }
  }

  previousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
      this.errorMessage = '';
    }
  }

  isCurrentStepValid(): boolean {
    const step1Fields = ['shopOwnerName', 'phone', 'shopName'];
    const step2Fields = ['verificationCode'];
    const step3Fields = ['password', 'confirmPassword', 'agreeToTerms'];

    let fieldsToValidate: string[] = [];

    switch (this.currentStep) {
      case 1:
        fieldsToValidate = step1Fields;
        break;
      case 2:
        fieldsToValidate = step2Fields;
        // Validate verification code
        if (this.registerForm.value.verificationCode !== this.generatedVerificationCode) {
          this.errorMessage = 'Mã xác thực không đúng';
          return false;
        }
        break;
      case 3:
        fieldsToValidate = step3Fields;
        // Validate password match
        if (this.registerForm.value.password !== this.registerForm.value.confirmPassword) {
          this.errorMessage = 'Mật khẩu xác nhận không khớp';
          return false;
        }
        break;
    }

    return fieldsToValidate.every(field => {
      const control = this.registerForm.get(field);
      return control && control.valid;
    });
  }

  markCurrentStepTouched(): void {
    const step1Fields = ['shopOwnerName', 'phone', 'shopName'];
    const step2Fields = ['shopAddress', 'businessCategoryId', 'verificationCode'];
    const step3Fields = ['password', 'confirmPassword', 'agreeToTerms'];

    let fieldsToMark: string[] = [];

    switch (this.currentStep) {
      case 1:
        fieldsToMark = step1Fields;
        break;
      case 2:
        fieldsToMark = step2Fields;
        break;
      case 3:
        fieldsToMark = step3Fields;
        break;
    }

    fieldsToMark.forEach(field => {
      this.registerForm.get(field)?.markAsTouched();
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.markFormGroupTouched(this.registerForm);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const categoryId = this.registerForm.value.businessCategoryId;
    const parsedCategoryId = categoryId ? (typeof categoryId === 'string' ? parseInt(categoryId, 10) : categoryId) : null;

    const registerData: RegisterRequest = {
      shopOwnerName: this.registerForm.value.shopOwnerName.trim(),
      phone: this.registerForm.value.phone.trim(),
      email: null,
      gender: null,
      dateOfBirth: null,
      address: null,
      taxCode: null,
      businessCategoryId: parsedCategoryId,
      shopName: this.registerForm.value.shopName.trim(),
      shopAddress: this.registerForm.value.shopAddress ? this.registerForm.value.shopAddress.trim() : null,
      shopDescription: null,
      password: this.registerForm.value.password,
      confirmPassword: this.registerForm.value.confirmPassword,
      termsAndConditionsAgreed: this.registerForm.value.agreeToTerms
    };

    console.log('📤 Data being sent:', JSON.stringify(registerData, null, 2));

    this.authService.register(registerData).subscribe({
      next: (response) => {
        this.isLoading = false;
        console.log('✅ Registration successful:', response);
        if (response.success) {
          this.successMessage = response.message || 'Đăng ký thành công!';
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        } else {
          this.errorMessage = response.message || 'Đăng ký thất bại';
        }
      },
      error: (error) => {
        this.isLoading = false;
        console.error('❌ Registration error:', error);
        
        if (error.status === 400 && error.error) {
          if (error.error.errors && Array.isArray(error.error.errors)) {
            this.errorMessage = error.error.errors.join('\n');
          } else if (error.error.errors && typeof error.error.errors === 'object') {
            const modelStateErrors: string[] = [];
            Object.keys(error.error.errors).forEach(key => {
              const messages = error.error.errors[key];
              if (Array.isArray(messages)) {
                modelStateErrors.push(...messages);
              }
            });
            this.errorMessage = modelStateErrors.join('\n');
          } else if (error.error.message) {
            this.errorMessage = error.error.message;
          } else {
            this.errorMessage = 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.';
          }
        } else {
          this.errorMessage = 'Có lỗi xảy ra. Vui lòng thử lại sau.';
        }
      }
    });
  }

  getStepButtonText(): string {
    if (this.currentStep < this.totalSteps) {
      return 'Tiếp tục';
    }
    return 'Hoàn tất đăng ký';
  }

  getProgressPercentage(): number {
    return (this.currentStep / this.totalSteps) * 100;
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.registerForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return 'Trường này là bắt buộc';
      if (field.errors['email']) return 'Email không hợp lệ';
      if (field.errors['minlength']) return `Tối thiểu ${field.errors['minlength'].requiredLength} ký tự`;
      if (field.errors['pattern']) return 'Định dạng không hợp lệ';
    }
    return '';
  }
  
}
