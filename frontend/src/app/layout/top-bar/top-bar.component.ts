import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-top-bar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './top-bar.component.html',
  styleUrl: './top-bar.component.css'
})
export class TopBarComponent {
  readonly currencies = ['USD ($)', 'EUR (€)', 'GBP (£)', 'PKR (₨)', 'AED (د.إ)', 'SAR (﷼)'];
  readonly selectedCurrency = signal('USD ($)');
  readonly isCurrencyDropdownOpen = signal(false);

  toggleCurrencyDropdown(): void {
    this.isCurrencyDropdownOpen.update(v => !v);
  }

  selectCurrency(currency: string): void {
    this.selectedCurrency.set(currency);
    this.isCurrencyDropdownOpen.set(false);
  }
}
