import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

const isApi = (url: string) => url.startsWith(environment.apiUrl);
const isAuthCall = (url: string) => /\/api\/Auth\/(login|register|refresh|logout|forgot-password|reset-password|confirm-email|resend-confirmation)/i.test(url);

const withToken = (req: HttpRequest<unknown>, token: string | null) =>
  token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

/** Adds the bearer token and transparently refreshes it once on a 401. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!isApi(req.url)) return next(req);
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(withToken(req, auth.token)).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status !== 401 || isAuthCall(req.url) || !auth.isLoggedIn()) return throwError(() => err);
      return auth.refresh().pipe(
        switchMap(token => next(withToken(req, token))),
        catchError(e => {
          auth.clear();
          router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
          return throwError(() => e);
        }),
      );
    }),
  );
};
