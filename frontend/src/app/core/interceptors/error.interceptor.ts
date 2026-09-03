import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';
import { Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred. Please try again.';

      if (error.error instanceof ErrorEvent) {
        // Client-side network error
        errorMessage = `Client error: ${error.error.message}`;
      } else {
        // Server-side response error
        switch (error.status) {
          case 0:
            errorMessage = 'Unable to connect to the server. Please check your internet connection or backend API status.';
            break;
          case 400:
            errorMessage = error.error?.message || 'Invalid request. Please verify your input.';
            break;
          case 401:
            errorMessage = 'Your session has expired. Please log in again.';
            // Redirect to login only if not already on login
            if (!router.url.includes('/login')) {
              router.navigate(['/login']);
            }
            break;
          case 403:
            errorMessage = 'You do not have permission to access this resource.';
            break;
          case 404:
            errorMessage = error.error?.message || 'The requested resource was not found.';
            break;
          case 409:
            errorMessage = error.error?.message || 'A conflict occurred. Please retry your request.';
            break;
          case 429:
            errorMessage = 'Too many requests. Please slow down and try again shortly.';
            break;
          case 500:
            errorMessage = 'Internal server error. Our engineering team has been notified.';
            break;
          default:
            errorMessage = error.error?.message || `Server error (${error.status}): Please try again.`;
            break;
        }
      }

      toastService.error('Request Error', errorMessage);
      return throwError(() => error);
    })
  );
};
