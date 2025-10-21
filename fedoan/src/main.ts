import { AppComponent } from './app/app.component';
import { bootstrapApplication } from '@angular/platform-browser';
import { config } from './app/app.config.server';

// Đây là hàm default export cho SSR
export default function bootstrap() {
  return bootstrapApplication(AppComponent, config);
}
