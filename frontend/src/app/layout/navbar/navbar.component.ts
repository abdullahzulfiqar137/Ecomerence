import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CategoryApiService } from '../../core/services/category-api.service';
import { Category } from '../../core/models/category.model';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {
  private readonly categoryApi = inject(CategoryApiService);

  readonly categories = signal<Category[]>([]);
  readonly isCategoriesDropdownOpen = signal(false);

  readonly navLinks = [
    { label: 'Home', path: '/' },
    { label: 'Shop All', path: '/products' },
    { label: 'Categories', path: '/categories' },
    { label: 'Brands', path: '/brands' },
    { label: 'Deals & Offers', path: '/products', query: { isFeatured: 'true' } },
    { label: 'Track Order', path: '/orders/tracking' }
  ];

  ngOnInit(): void {
    this.categoryApi.getCategories().subscribe({
      next: (data) => this.categories.set(data),
      error: () => {}
    });
  }

  toggleCategoriesDropdown(): void {
    this.isCategoriesDropdownOpen.update(v => !v);
  }

  closeDropdown(): void {
    this.isCategoriesDropdownOpen.set(false);
  }
}
