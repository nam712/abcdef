import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  currentDate = new Date();
  notificationCount = 3;
  isScrolled = false;
  isMobileMenuOpen = false;

  user = {
    name: 'Nguyễn Văn A'
  };

  stats = {
    totalProjects: 12,
    completedTasks: 48,
    avgProgress: 75,
    teamMembers: 8
  };

  recentProjects = [
    {
      id: 1,
      name: 'Website E-commerce',
      description: 'Phát triển website bán hàng trực tuyến với đầy đủ tính năng',
      progress: 85,
      status: 'active',
      deadline: new Date('2024-02-15'),
      team: [
        { name: 'Nguyễn Văn B' },
        { name: 'Trần Thị C' },
        { name: 'Lê Văn D' }
      ]
    },
    {
      id: 2,
      name: 'App Mobile Banking',
      description: 'Ứng dụng ngân hàng di động với bảo mật cao',
      progress: 60,
      status: 'active',
      deadline: new Date('2024-03-20'),
      team: [
        { name: 'Phạm Văn E' },
        { name: 'Hoàng Thị F' }
      ]
    },
    {
      id: 3,
      name: 'Dashboard Analytics',
      description: 'Hệ thống phân tích dữ liệu và báo cáo',
      progress: 100,
      status: 'completed',
      deadline: new Date('2024-01-30'),
      team: [
        { name: 'Vũ Văn G' },
        { name: 'Đặng Thị H' },
        { name: 'Bùi Văn I' },
        { name: 'Mai Thị J' }
      ]
    }
  ];

  recentActivities = [
    {
      id: 1,
      type: 'task',
      message: 'Hoàn thành task "Thiết kế giao diện đăng nhập"',
      timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000)
    },
    {
      id: 2,
      type: 'project',
      message: 'Cập nhật tiến độ dự án "Website E-commerce" lên 85%',
      timestamp: new Date(Date.now() - 4 * 60 * 60 * 1000)
    },
    {
      id: 3,
      type: 'team',
      message: 'Thêm thành viên mới vào team "App Mobile Banking"',
      timestamp: new Date(Date.now() - 6 * 60 * 60 * 1000)
    },
    {
      id: 4,
      type: 'project',
      message: 'Hoàn thành dự án "Dashboard Analytics"',
      timestamp: new Date(Date.now() - 8 * 60 * 60 * 1000)
    }
  ];

  features = [
    {
      title: 'Quản lý Task',
      description: 'Tạo, phân công và theo dõi tiến độ các task một cách trực quan với giao diện kéo thả đơn giản.',
      icon: 'fa-tasks',
      iconClass: 'tasks'
    },
    {
      title: 'Cộng tác Team',
      description: 'Làm việc nhóm hiệu quả với chat real-time, chia sẻ file và thông báo tức thì.',
      icon: 'fa-users',
      iconClass: 'collaboration'
    },
    {
      title: 'Báo cáo & Analytics',
      description: 'Phân tích hiệu suất dự án với các biểu đồ và báo cáo chi tiết, giúp ra quyết định chính xác.',
      icon: 'fa-chart-bar',
      iconClass: 'analytics'
    },
    {
      title: 'Quản lý Timeline',
      description: 'Lập kế hoạch dự án với timeline rõ ràng, theo dõi milestone và deadline quan trọng.',
      icon: 'fa-calendar-alt',
      iconClass: 'timeline'
    },
    {
      title: 'Tích hợp đa nền tảng',
      description: 'Kết nối với các công cụ quen thuộc như Slack, Google Drive, GitHub và nhiều hơn nữa.',
      icon: 'fa-plug',
      iconClass: 'integration'
    },
    {
      title: 'Bảo mật cao',
      description: 'Dữ liệu được mã hóa và bảo vệ với các tiêu chuẩn bảo mật quốc tế, backup tự động.',
      icon: 'fa-shield-alt',
      iconClass: 'security'
    }
  ];

  benefits = [
    {
      title: 'Tăng năng suất lên 40%',
      description: 'Tự động hóa quy trình làm việc và giảm thiểu thời gian quản lý thủ công'
    },
    {
      title: 'Miễn phí cho team nhỏ',
      description: 'Sử dụng miễn phí cho team dưới 5 người với đầy đủ tính năng cơ bản'
    },
    {
      title: 'Hỗ trợ 24/7',
      description: 'Đội ngũ chăm sóc khách hàng luôn sẵn sàng hỗ trợ bạn mọi lúc mọi nơi'
    },
    {
      title: 'Dễ sử dụng',
      description: 'Giao diện thân thiện, dễ học và không cần đào tạo phức tạp'
    }
  ];

  constructor(private router: Router) {}

  ngOnInit(): void {}

  ngOnDestroy(): void {}

  @HostListener('window:scroll', ['$event'])
  onWindowScroll() {
    this.isScrolled = window.pageYOffset > 50;
  }

  toggleMobileMenu() {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu() {
    this.isMobileMenuOpen = false;
  }

  navigateToLogin() {
    console.log('Navigating to login...');
    this.router.navigate(['/login']);
  }

  navigateToRegister() {
    console.log('Navigating to register...');
    this.router.navigate(['/register']);
  }

  viewDemo() {
    console.log('Demo button clicked!');
    alert('Chức năng demo đang được phát triển');
  }
}