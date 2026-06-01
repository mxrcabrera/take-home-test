import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';

import { inject } from '@angular/core';

import { Router } from '@angular/router';

import { catchError, throwError } from 'rxjs';

import { AuthService } from '../services/auth.service';



export const authInterceptor: HttpInterceptorFn = (req, next) => {

  const authService = inject(AuthService);

  const router = inject(Router);

  // Use withCredentials to include httpOnly cookies for better security
  // This prevents XSS attacks since cookies are not accessible via JavaScript
  req = req.clone({
    withCredentials: true
  });

  return next(req).pipe(

    catchError((error: HttpErrorResponse) => {

      if (error.status === 401) {

        authService.logout();

        router.navigate(['/login']);

      }

      return throwError(() => error);

    })

  );

};

