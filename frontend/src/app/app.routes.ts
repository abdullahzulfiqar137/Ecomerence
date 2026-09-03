import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent),
    title: 'Aura Global | Premium International E-Commerce'
  },
  {
    path: '**',
    redirectTo: ''
  }
];
