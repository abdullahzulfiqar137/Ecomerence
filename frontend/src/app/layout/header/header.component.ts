import { Component, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  @Output() openMobileMenu = new EventEmitter<void>();

  searchQuery = signal('');
  cartCount = signal(0);
  wishlistCount = signal(0);
  isUserMenuOpen = signal(false);

  constructor(private readonly router: Router) {}

  onSearchSubmit(): void {
    const query = this.searchQuery().trim();
    if (query) {
      this.router.navigate(['/search'], { queryParams: { q: query } });
    }
  }

  toggleUserMenu(): void {
    this.isUserMenuOpen.update(v => !v);
  }

  toggleMobileMenu(): void {
    this.openMobileMenu.emit();
  }
}
