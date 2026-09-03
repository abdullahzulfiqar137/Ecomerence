import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css'
})
export class FooterComponent {
  newsletterEmail = signal('');
  newsletterSubscribed = signal(false);
  readonly currentYear = new Date().getFullYear();

  subscribeNewsletter(): void {
    if (this.newsletterEmail().trim()) {
      this.newsletterSubscribed.set(true);
    }
  }
}
