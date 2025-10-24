import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-complete-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './complete-profile.component.html',
  styleUrls: ['./complete-profile.component.css']
})
export class CompleteProfileComponent implements OnInit {
  profileForm: FormGroup;
  isLoading = false;
  showPassword = false;
  showSuccessMessage = false;

  businessSectors = [
    { value: 'technology', label: 'Công nghệ thông tin' },
    { value: 'finance', label: 'Tài chính - Ngân hàng' },
    { value: 'healthcare', label: 'Y tế - Chăm sóc sức khỏe' },
    { value: 'education', label: 'Giáo dục - Đào tạo' },
    { value: 'manufacturing', label: 'Sản xuất - Chế tạo' },
    { value: 'retail', label: 'Bán lẻ - Thương mại' },
    { value: 'construction', label: 'Xây dựng - Bất động sản' },
    { value: 'food', label: 'Thực phẩm - Đồ uống' },
    { value: 'tourism', label: 'Du lịch - Khách sạn' },
    { value: 'logistics', label: 'Vận tải - Logistics' },
    { value: 'media', label: 'Truyền thông - Giải trí' },
    { value: 'agriculture', label: 'Nông nghiệp - Thủy sản' },
    { value: 'energy', label: 'Năng lượng - Môi trường' },
    { value: 'consulting', label: 'Tư vấn - Dịch vụ' },
    { value: 'automotive', label: 'Ô tô - Xe máy' },
    { value: 'textile', label: 'Dệt may - Thời trang' },
    { value: 'electronics', label: 'Điện tử - Viễn thông' },
    { value: 'chemical', label: 'Hóa chất - Dược phẩm' },
    { value: 'sports', label: 'Thể thao - Giải trí' },
    { value: 'beauty', label: 'Làm đẹp - Chăm sóc cá nhân' },
    { value: 'other', label: 'Khác' }
  ];

  constructor(
    private formBuilder: FormBuilder,
    private router: Router
  ) {
    this.profileForm = this.formBuilder.group({
      businessSector: ['', [Validators.required]],
      companyName: ['', [Validators.required, Validators.minLength(2)]],
      password: ['', [Validators.required, Validators.minLength(8), this.passwordStrengthValidator]]
    });
  }

  ngOnInit(): void {}

  passwordStrengthValidator(control: any) {
    const value = control.value;
    if (!value) return null;
    
    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumeric = /[0-9]/.test(value);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(value);
    
    const valid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecialChar;
    
    if (!valid) {
      return { passwordStrength: true };
    }
    
    return null;
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.profileForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.profileForm.get(fieldName);
    if (field && field.errors && (field.dirty || field.touched)) {
      if (field.errors['required']) return `Vui lòng ${this.getFieldAction(fieldName)} ${this.getFieldLabel(fieldName)}`;
      if (field.errors['minlength']) return `${this.getFieldLabel(fieldName)} quá ngắn`;
      if (field.errors['passwordStrength']) return 'Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt';
    }
    return '';
  }

  private getFieldAction(fieldName: string): string {
    const actions: { [key: string]: string } = {
      'businessSector': 'chọn',
      'companyName': 'nhập',
      'password': 'nhập'
    };
    return actions[fieldName] || 'nhập';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      'businessSector': 'ngành hàng kinh doanh',
      'companyName': 'tên công ty',
      'password': 'mật khẩu'
    };
    return labels[fieldName] || fieldName;
  }

  onSubmit(): void {
    if (this.profileForm.valid) {
      this.isLoading = true;
      
      setTimeout(() => {
        console.log('Profile completed:', this.profileForm.value);
        this.isLoading = false;
        this.showSuccessMessage = true;
        
        // Show success message for 2 seconds then navigate to login
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      }, 2000);
    } else {
      Object.keys(this.profileForm.controls).forEach(key => {
        this.profileForm.get(key)?.markAsTouched();
      });
    }
  }

  skipProfile(): void {
    this.router.navigate(['/dashboard']);
  }
}
