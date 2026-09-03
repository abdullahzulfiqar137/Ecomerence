import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { ApiService } from '../services/api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { STORAGE_KEYS } from '../constants/storage-keys';
import { User, AuthResponse } from '../models/user.model';
import { LoginDto, RegisterDto, ForgotPasswordDto } from './auth.models';
import { ToastService } from '../services/toast.service';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly currentUser = signal<User | null>(this.getStoredUser());
  readonly isAuthenticated = computed(() => !!this.currentUser());
  readonly isAdmin = computed(() => {
    const user = this.currentUser();
    return !!user && (user.roles?.includes('Admin') || user.roles?.includes('SuperAdmin'));
  });

  constructor() {
    if (this.hasToken()) {
      this.fetchCurrentUser().subscribe();
    }
  }

  login(credentials: LoginDto): Observable<ApiResponse<AuthResponse>> {
    return this.api.post<AuthResponse>(API_ENDPOINTS.auth.login, credentials).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.setSession(res.data);
          this.toast.success('Welcome Back!', `Signed in as ${res.data.user.firstName}`);
        }
      })
    );
  }

  register(data: RegisterDto): Observable<ApiResponse<AuthResponse>> {
    return this.api.post<AuthResponse>(API_ENDPOINTS.auth.register, data).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.setSession(res.data);
          this.toast.success('Account Created', 'Your luxury account is ready. Welcome to Aura Global!');
        }
      })
    );
  }

  forgotPassword(data: ForgotPasswordDto): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(API_ENDPOINTS.auth.forgotPassword, data);
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(STORAGE_KEYS.accessToken);
      localStorage.removeItem(STORAGE_KEYS.refreshToken);
      localStorage.removeItem(STORAGE_KEYS.currentUser);
    }
    this.currentUser.set(null);
    this.toast.info('Logged Out', 'You have been safely signed out.');
    this.router.navigate(['/login']);
  }

  fetchCurrentUser(): Observable<ApiResponse<User> | null> {
    if (!this.hasToken()) return of(null);

    return this.api.get<User>(API_ENDPOINTS.auth.me).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.currentUser.set(res.data);
          if (typeof window !== 'undefined') {
            localStorage.setItem(STORAGE_KEYS.currentUser, JSON.stringify(res.data));
          }
        }
      }),
      catchError(() => {
        this.logout();
        return of(null);
      })
    );
  }

  private setSession(auth: AuthResponse): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(STORAGE_KEYS.accessToken, auth.accessToken);
      localStorage.setItem(STORAGE_KEYS.refreshToken, auth.refreshToken);
      localStorage.setItem(STORAGE_KEYS.currentUser, JSON.stringify(auth.user));
    }
    this.currentUser.set(auth.user);
  }

  private hasToken(): boolean {
    if (typeof window === 'undefined') return false;
    return !!localStorage.getItem(STORAGE_KEYS.accessToken);
  }

  private getStoredUser(): User | null {
    if (typeof window === 'undefined') return null;
    const json = localStorage.getItem(STORAGE_KEYS.currentUser);
    if (!json) return null;
    try {
      return JSON.parse(json) as User;
    } catch {
      return null;
    }
  }
}
