import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { UserStateService } from '../services/user-state.service';

/** Attaches the JWT bearer token to API calls and signs the user out on a 401. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const userState = inject(UserStateService);
  const router = inject(Router);

  const isAuthEndpoint =
    req.url.includes('/api/users/login') || req.url.includes('/api/users/register');
  const token = userState.token$();

  const authReq =
    token && !isAuthEndpoint
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isAuthEndpoint) {
        userState.logout();
        router.navigate(['/tours']);
      }
      return throwError(() => error);
    })
  );
};
