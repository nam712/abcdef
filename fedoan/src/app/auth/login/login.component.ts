import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="login-container">
      <!-- Left side - Image -->
      <div class="login-image-side">
        <div class="image-overlay">
          <div class="brand-info">
            <h1>FeDoan</h1>
            <p>Nền tảng quản lý dự án hiện đại</p>
            <div class="features-highlight">
              <div class="feature-item">
                <i class="fas fa-check"></i>
                <span>Quản lý cửa hàng hiệu quả</span>
              </div>
              <div class="feature-item">
                <i class="fas fa-check"></i>
                <span>Báo cáo chi tiết </span>
              </div>
              <div class="feature-item">
                <i class="fas fa-check"></i>
                <span>Liên hệ 190000 </span>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Right side - Login Form -->
      <div class="login-form-side">
        <div class="login-card">
          <h2>Đăng nhập</h2>
          <p class="welcome-text">Chào mừng bạn quay trở lại!</p>
          
          <form (ngSubmit)="onSubmit()">
            <div class="form-group">
              <label for="email">Email</label>
              <input type="email" id="email" placeholder="Nhập email của bạn" required>
            </div>
            <div class="form-group">
              <label for="password">Mật khẩu</label>
              <input type="password" id="password" placeholder="Nhập mật khẩu" required>
            </div>
            
            <div class="form-options">
              <label class="checkbox">
                <input type="checkbox">
                <span>Ghi nhớ đăng nhập</span>
              </label>
              <a href="#" class="forgot-password">Quên mật khẩu?</a>
            </div>
            
            <button type="submit" class="btn-primary">Đăng nhập</button>
          </form>
          
          <div class="login-footer">
            <p>Chưa có tài khoản? <a (click)="navigateToRegister()">Đăng ký ngay</a></p>
            <p class="back-home"><a (click)="navigateToHome()">Quay lại trang chủ</a></p>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      height: 100vh; /* Changed from min-height to fixed height */
      display: flex;
      background: white;
      font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
      max-width: 100vw;
      overflow: hidden;
    }
    
    .login-image-side {
      flex: 0 0 50%;
      height: 100vh; /* Changed to full viewport height */
      background-image: url('https://toigingiuvedep.vn/wp-content/uploads/2023/03/anh-nguoi-dep-trung-quoc.jpg');
      background-size: cover;
      background-position: center;
      background-repeat: no-repeat;
      position: relative;
      display: block;
    }
    
    .image-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.4);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 40px;
    }
    
    .brand-info {
      text-align: center;
      color: white;
      animation: fadeInUp 1s ease-out 0.3s both;
      max-width: 450px;
    }
    
    .brand-info h1 {
      font-size: clamp(1.6rem, 2.8vw, 2.8rem); /* Further reduced */
      font-weight: 800;
      margin-bottom: 10px;
      text-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
      background: linear-gradient(135deg, #ffffff, #f0f4f7);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }
    
    .brand-info p {
      font-size: clamp(0.75rem, 1.3vw, 0.95rem); /* Further reduced */
      margin-bottom: 18px; /* Reduced from 20px */
      opacity: 0.95;
      font-weight: 300;
    }
    
    .features-highlight {
      display: flex;
      flex-direction: column;
      gap: 10px; /* Reduced from 12px */
      max-width: 100%;
    }
    
    .feature-item {
      display: flex;
      align-items: center;
      gap: 8px; /* Reduced from 10px */
      font-size: clamp(0.7rem, 1.1vw, 0.8rem); /* Further reduced */
      font-weight: 400;
      padding: 6px 10px; /* Reduced from 8px 12px */
      background: rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      backdrop-filter: blur(10px);
      border: 1px solid rgba(255, 255, 255, 0.2);
      transition: all 0.3s ease;
      animation: fadeInUp 1s ease-out calc(0.5s + var(--delay, 0s)) both;
    }
    
    .feature-item:nth-child(1) { --delay: 0.1s; }
    .feature-item:nth-child(2) { --delay: 0.2s; }
    .feature-item:nth-child(3) { --delay: 0.3s; }
    
    .feature-item:hover {
      background: rgba(255, 255, 255, 0.2);
      transform: translateX(10px);
    }
    
    .feature-item i {
      width: 20px; /* Reduced from 22px */
      height: 20px;
      background: linear-gradient(135deg, #ff6b6b, #ee5a24);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 9px; /* Reduced from 10px */
      box-shadow: 0 4px 15px rgba(255, 107, 107, 0.3);
    }
    
    .login-form-side {
      flex: 0 0 50%;
      height: 100vh; /* Changed to full viewport height */
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 20px;
      background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
      position: relative;
      animation: slideInRight 0.8s ease-out;
      overflow-y: auto;
    }
    
    .login-card {
      background: rgba(255, 255, 255, 0.98);
      backdrop-filter: blur(20px);
      padding: clamp(20px, 2.5vw, 30px); /* Reduced from clamp(25px, 3vw, 35px) */
      border-radius: 18px; /* Reduced radius */
      box-shadow: 
        0 25px 80px rgba(0, 0, 0, 0.1),
        0 0 0 1px rgba(255, 255, 255, 0.5);
      width: 100%;
      max-width: 320px; /* Reduced from 350px */
      position: relative;
      z-index: 2;
      animation: fadeInUp 1s ease-out 0.4s both;
      transition: all 0.3s ease;
      margin: auto;
    }
    
    .login-card:hover {
      transform: translateY(-5px);
      box-shadow: 
        0 35px 100px rgba(0, 0, 0, 0.15),
        0 0 0 1px rgba(255, 255, 255, 0.7);
    }
    
    h2 {
      font-size: clamp(1.3rem, 2.8vw, 1.7rem); /* Further reduced */
      font-weight: 700;
      margin-bottom: 5px;
      color: #1a202c;
      text-align: center;
      background: linear-gradient(135deg, #667eea, #764ba2);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }
    
    .welcome-text {
      color: #64748b;
      margin-bottom: 16px; /* Reduced from 18px */
      font-size: clamp(0.75rem, 1.3vw, 0.85rem); /* Further reduced */
      text-align: center;
      font-weight: 400;
    }
    
    .form-group {
      margin-bottom: 12px; /* Reduced from 15px */
      position: relative;
    }
    
    label {
      display: block;
      margin-bottom: 5px; /* Reduced from 6px */
      font-weight: 600;
      color: #374151;
      font-size: 11px; /* Reduced from 12px */
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    
    input[type="email"],
    input[type="password"] {
      width: 100%;
      padding: 10px 12px; /* Reduced from 12px 14px */
      border: 2px solid #e2e8f0;
      border-radius: 8px; /* Further reduced radius */
      font-size: clamp(11px, 1.4vw, 13px); /* Further reduced */
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      background: #ffffff;
      position: relative;
      box-sizing: border-box;
    }
    
    input:focus {
      outline: none;
      border-color: #667eea;
      background: white;
      box-shadow: 
        0 0 0 4px rgba(102, 126, 234, 0.1),
        0 8px 25px rgba(102, 126, 234, 0.15);
      transform: translateY(-2px);
    }
    
    input::placeholder {
      color: #94a3b8;
      font-weight: 400;
    }
    
    .form-options {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 18px; /* Reduced from 22px */
      flex-wrap: wrap;
      gap: 8px; /* Reduced from 10px */
    }
    
    .checkbox {
      display: flex;
      align-items: center;
      gap: 5px; /* Reduced from 6px */
      font-size: 11px; /* Reduced from 12px */
      color: #64748b;
      cursor: pointer;
      font-weight: 500;
      transition: color 0.2s;
    }
    
    .checkbox:hover {
      color: #374151;
    }
    
    .checkbox input {
      width: 13px; /* Reduced from 14px */
      height: 13px;
      margin: 0;
      accent-color: #667eea;
    }
    
    .forgot-password {
      color: #667eea;
      text-decoration: none;
      font-size: 11px; /* Reduced from 12px */
      font-weight: 600;
      transition: all 0.2s;
      position: relative;
    }
    
    .forgot-password::after {
      content: '';
      position: absolute;
      width: 0;
      height: 2px;
      bottom: -2px;
      left: 0;
      background: #667eea;
      transition: width 0.3s ease;
    }
    
    .forgot-password:hover::after {
      width: 100%;
    }
    
    .btn-primary {
      width: 100%;
      padding: 10px; /* Reduced from 12px */
      background: linear-gradient(135deg, #667eea, #764ba2);
      color: white;
      border: none;
      border-radius: 8px; /* Further reduced */
      font-size: clamp(11px, 1.4vw, 13px); /* Further reduced */
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      margin-bottom: 12px; /* Reduced from 15px */
      position: relative;
      overflow: hidden;
    }
    
    .btn-primary::before {
      content: '';
      position: absolute;
      top: 0;
      left: -100%;
      width: 100%;
      height: 100%;
      background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.3), transparent);
      transition: left 0.6s ease;
    }
    
    .btn-primary:hover {
      transform: translateY(-3px);
      box-shadow: 0 15px 35px rgba(102, 126, 234, 0.4);
    }
    
    .btn-primary:hover::before {
      left: 100%;
    }
    
    .btn-primary:active {
      transform: translateY(-1px);
    }
    
    .login-footer {
      text-align: center;
      border-top: 1px solid #e2e8f0;
      padding-top: 12px; /* Reduced from 15px */
    }
    
    .login-footer p {
      margin-bottom: 6px; /* Reduced from 8px */
      color: #64748b;
      font-size: 11px; /* Reduced from 12px */
    }
    
    .login-footer a {
      color: #667eea;
      cursor: pointer;
      text-decoration: none;
      font-weight: 600;
      transition: all 0.2s;
      position: relative;
    }
    
    .login-footer a::after {
      content: '';
      position: absolute;
      width: 0;
      height: 2px;
      bottom: -2px;
      left: 50%;
      background: #667eea;
      transition: all 0.3s ease;
      transform: translateX(-50%);
    }
    
    .login-footer a:hover::after {
      width: 100%;
    }
    
    .back-home a {
      color: #94a3b8;
      font-size: 10px; /* Reduced from 11px */
      font-weight: 500;
    }
    
    /* Animations */
    @keyframes slideInLeft {
      from {
        opacity: 0;
        transform: translateX(-100px);
      }
      to {
        opacity: 1;
        transform: translateX(0);
      }
    }
    
    @keyframes slideInRight {
      from {
        opacity: 0;
        transform: translateX(100px);
      }
      to {
        opacity: 1;
        transform: translateX(0);
      }
    }
    
    @keyframes fadeInUp {
      from {
        opacity: 0;
        transform: translateY(30px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    
    /* Responsive */
    @media (max-width: 1200px) {
      .login-card {
        max-width: 300px; /* Reduced from 320px */
        padding: 25px 18px; /* Reduced padding */
      }
    }
    
    @media (max-width: 768px) {
      .login-container {
        flex-direction: column;
        height: 100vh;
      }
      
      .login-image-side {
        flex: 0 0 35vh; /* Increased for better proportion */
        height: 35vh;
      }
      
      .login-form-side {
        flex: 1;
        height: 65vh; /* Adjusted accordingly */
        padding: 12px;
      }
      
      .login-card {
        padding: 20px 15px; /* Further reduced */
        border-radius: 15px;
        max-width: 100%;
      }
      
      .image-overlay {
        padding: 15px; /* Further reduced */
      }
      
      .features-highlight {
        gap: 8px;
      }
      
      .feature-item {
        padding: 6px 10px; /* Further reduced */
        gap: 8px;
      }
      
      .form-options {
        flex-direction: column;
        align-items: flex-start;
        gap: 6px;
      }
    }
    
    @media (max-width: 480px) {
      .login-container {
        height: 100vh;
      }
      
      .login-image-side {
        flex: 0 0 30vh; /* Adjusted for small screens */
        height: 30vh;
      }
      
      .login-form-side {
        flex: 1;
        height: 70vh;
        padding: 8px;
      }
      
      .login-card {
        padding: 18px 12px; /* Further reduced */
      }
      
      .brand-info h1 {
        margin-bottom: 6px;
      }
      
      .brand-info p {
        margin-bottom: 12px;
      }
      
      .features-highlight {
        gap: 6px;
      }
    }
    
    /* Large screens */
    @media (min-width: 1400px) {
      .login-card {
        max-width: 380px; /* Reasonable size for large screens */
        padding: 40px 35px;
      }
      
      .image-overlay {
        padding: 40px;
      }
    }
  `]
})
export class LoginComponent {
  constructor(private router: Router) {}

  onSubmit() {
    console.log('Login submitted');
    // TODO: Implement login logic
  }

  navigateToRegister() {
    this.router.navigate(['/register']);
  }

  navigateToHome() {
    this.router.navigate(['/']);
  }
}
