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
    // Bắt đầu theo dõi tồn kho khi app khởi động
    this.inventoryAlertService.startMonitoring();
  }

  ngOnDestroy(): void {
    // Dừng theo dõi khi app đóng
    this.inventoryAlertService.stopMonitoring();
  }
}
