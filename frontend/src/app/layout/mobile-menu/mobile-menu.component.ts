import { Component, Input, Output, EventEmitter, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CategoryApiService } from '../../core/services/category-api.service';
import { Category } from '../../core/models/category.model';

@Component({
  selector: 'app-mobile-menu',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './mobile-menu.component.html',
  styleUrl: './mobile-menu.component.css'
})
export class MobileMenuComponent implements OnInit {
  @Input() isOpen = false;
  @Output() closeMenu = new EventEmitter<void>();

  private readonly categoryApi = inject(CategoryApiService);
  private readonly router = inject(Router);

  readonly categories = signal<Category[]>([]);
  searchQuery = signal('');

  ngOnInit(): void {
    this.categoryApi.getCategories().subscribe({
      next: (data) => this.categories.set(data),
      error: () => {}
    });
  }

  onSearch(): void {
    const q = this.searchQuery().trim();
    if (q) {
      this.close();
      this.router.navigate(['/search'], { queryParams: { q } });
    }
  }

  close(): void {
    this.closeMenu.emit();
  }
}
