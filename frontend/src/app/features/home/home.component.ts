import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  readonly features = [
    {
      title: 'Global Express Delivery',
      desc: 'Complimentary expedited shipping on international orders over $150 with tracked transit.',
      icon: 'globe'
    },
    {
      title: 'Authenticated Luxury',
      desc: 'Every item certified by verified master appraisers with serialized tamper-proof proof.',
      icon: 'shield'
    },
    {
      title: 'Encrypted Checkout',
      desc: 'Multi-currency settlement backed by Level-1 PCI DSS tokenized encryption standards.',
      icon: 'lock'
    },
    {
      title: '30-Day Privilege Return',
      desc: 'Enjoy effortless complimentary returns with pre-printed worldwide return labels.',
      icon: 'rotate'
    }
  ];
}
