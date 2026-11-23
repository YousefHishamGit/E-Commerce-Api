import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map, of, shareReplay, tap, finalize, catchError, throwError } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, AuthResponse, RegisterRequest, Role, User } from './models';
import { environment } from '../../environments/environment';

const KEY = 'mazaj.session';

interface Session { userId: string; email: string; fullName: string; role: Role; emailConfirmed: boolean; token: string; refreshToken: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = inject(ApiService);
  private http = inject(HttpClient);
  private router = inject(Router);

  private session = signal<Session | null>(this.read());
  private refreshing$: Observable<string> | null = null;

  readonly user = this.session.asReadonly();
  readonly isLoggedIn = computed(() => !!this.session());
  readonly isAdmin = computed(() => this.session()?.role === 'Admin');
  readonly firstName = computed(() => this.session()?.fullName.split(' ')[0] ?? '');
  get token() { return this.session()?.token ?? null; }

  login(email: string, password: string) {
    return this.api.post<AuthResponse>('Auth/login', { email, password }).pipe(tap(r => this.store(r)));
  }

  register(req: RegisterRequest) {
    return this.api.post<AuthResponse>('Auth/register', req).pipe(tap(r => this.store(r)));
  }

  /** Single-flight refresh: concurrent 401s share one request. */
  refresh(): Observable<string> {
    const s = this.session();
    if (!s) return throwError(() => new Error('No session'));
    if (!this.refreshing$) {
      this.refreshing$ = this.http
        .post<ApiResponse<AuthResponse>>(`${this.api.base}/api/Auth/refresh`, { refreshToken: s.refreshToken })
        .pipe(
          map(r => r.data),
          tap(r => this.store(r)),
          map(r => r.token),
          finalize(() => (this.refreshing$ = null)),
          shareReplay(1),
        );
    }
    return this.refreshing$;
  }

  logout(redirect = true) {
    const s = this.session();
    if (s) {
      this.http.post(`${this.api.base}/api/Auth/logout`, { refreshToken: s.refreshToken }).pipe(catchError(() => of(null))).subscribe();
    }
    this.clear();
    if (redirect) this.router.navigateByUrl('/');
  }

  /** Clears local session only (used when the server already rejected our tokens). */
  clear() {
    this.session.set(null);
    try { localStorage.removeItem(KEY); } catch { /* storage unavailable */ }
  }

  me() { return this.api.get<User>('Auth/me'); }
  updateProfile(body: Partial<Pick<User, 'firstName' | 'lastName' | 'phone'>>) {
    return this.api.put<User>('Auth/me', body).pipe(
      tap(u => this.patch({ fullName: `${u.firstName} ${u.lastName}` })),
    );
  }
  changePassword(body: { currentPassword: string; newPassword: string; confirmNewPassword: string }) {
    return this.api.post<unknown>('Auth/change-password', body);
  }
  forgotPassword(email: string) { return this.api.post<unknown>('Auth/forgot-password', { email }); }
  resetPassword(body: { email: string; token: string; newPassword: string; confirmNewPassword: string }) {
    return this.api.post<unknown>('Auth/reset-password', body);
  }
  confirmEmail(email: string, token: string) {
    return this.api.post<unknown>('Auth/confirm-email', { email, token }).pipe(tap(() => this.patch({ emailConfirmed: true })));
  }
  resendConfirmation(email: string) { return this.api.post<unknown>('Auth/resend-confirmation', { email }); }

  private store(r: AuthResponse) {
    const s: Session = {
      userId: r.userId, email: r.email, fullName: r.fullName, role: r.role,
      emailConfirmed: r.emailConfirmed, token: r.token, refreshToken: r.refreshToken,
    };
    this.session.set(s);
    try { localStorage.setItem(KEY, JSON.stringify(s)); } catch { /* storage unavailable */ }
  }

  private patch(p: Partial<Session>) {
    const s = this.session();
    if (!s) return;
    const next = { ...s, ...p };
    this.session.set(next);
    try { localStorage.setItem(KEY, JSON.stringify(next)); } catch { /* ignore */ }
  }

  private read(): Session | null {
    try {
      const raw = localStorage.getItem(KEY);
      return raw ? (JSON.parse(raw) as Session) : null;
    } catch { return null; }
  }
}
