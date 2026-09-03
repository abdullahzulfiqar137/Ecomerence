import { Routes } from '@angular/router';
import { guestGuard } from './core/guards/guest.guard';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent),
    title: 'Aura Global | Premium International Store'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/authentication/login/login.component').then(m => m.LoginComponent),
    canActivate: [guestGuard],
    title: 'Sign In | Aura Global'
  },
  {
    path: 'register',
    loadComponent: () => import('./features/authentication/register/register.component').then(m => m.RegisterComponent),
    canActivate: [guestGuard],
    title: 'Create Luxury Account | Aura Global'
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/authentication/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent),
    canActivate: [guestGuard],
    title: 'Reset Password | Aura Global'
  },
  {
    path: '**',
    redirectTo: ''
  }
];
