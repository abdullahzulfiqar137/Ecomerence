import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TopBarComponent } from './layout/top-bar/top-bar.component';
import { HeaderComponent } from './layout/header/header.component';
import { NavbarComponent } from './layout/navbar/navbar.component';
import { FooterComponent } from './layout/footer/footer.component';
import { MobileMenuComponent } from './layout/mobile-menu/mobile-menu.component';
import { ToastComponent } from './shared/components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    TopBarComponent,
    HeaderComponent,
    NavbarComponent,
    FooterComponent,
    MobileMenuComponent,
    ToastComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'Aura Global';
  isMobileMenuOpen = signal(false);
}
