import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { provideHttpClient } from '@angular/common/http'; // 👈 Thêm import này

// Đây là hàm default export cho SSR
export default function bootstrap() {
  return bootstrapApplication(AppComponent, appConfig);
}

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient() // 👈 Thêm dòng này để cung cấp HttpClient
  ]
})
  .catch(err => console.error(err));
