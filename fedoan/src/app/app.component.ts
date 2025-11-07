import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { InventoryAlertService } from './services/inventory-alert.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet></router-outlet>',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'fedoan';

  constructor(private inventoryAlertService: InventoryAlertService) {}

  ngOnInit(): void {
    // Tắt tạm thời notification service vì bảng chưa tồn tại
    const token = localStorage.getItem('token');
    if (token) {
      // Tạm thời comment để tránh lỗi
      // this.inventoryAlertService.startMonitoring();
      console.log('⚠️ Inventory monitoring disabled - notifications table not exist');
    }
  }

  ngOnDestroy(): void {
    // Dừng theo dõi khi app đóng
    this.inventoryAlertService.stopMonitoring();
  }
}
