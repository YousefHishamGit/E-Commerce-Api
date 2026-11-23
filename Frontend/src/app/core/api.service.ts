import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './models';

type Params = Record<string, string | number | boolean | null | undefined>;

/** Thin wrapper that unwraps the API's `{ success, data, message }` envelope. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  readonly base = environment.apiUrl.replace(/\/$/, '');

  get<T>(path: string, params?: Params): Observable<T> {
    return this.http.get<ApiResponse<T>>(this.url(path), { params: this.toParams(params) }).pipe(map(r => r.data));
  }
  post<T>(path: string, body: unknown = {}): Observable<T> {
    return this.http.post<ApiResponse<T>>(this.url(path), body).pipe(map(r => r.data));
  }
  put<T>(path: string, body: unknown = {}): Observable<T> {
    return this.http.put<ApiResponse<T>>(this.url(path), body).pipe(map(r => r.data));
  }
  patch<T>(path: string, body: unknown = {}): Observable<T> {
    return this.http.patch<ApiResponse<T>>(this.url(path), body).pipe(map(r => r.data));
  }
  delete<T>(path: string): Observable<T> {
    return this.http.delete<ApiResponse<T>>(this.url(path)).pipe(map(r => r.data));
  }
  upload<T>(path: string, form: FormData): Observable<T> {
    return this.http.post<ApiResponse<T>>(this.url(path), form).pipe(map(r => r.data));
  }

  /** Turns a relative `/uploads/x.jpg` into an absolute URL; returns a placeholder if empty. */
  img(path?: string | null): string {
    if (!path) return PLACEHOLDER;
    if (/^(https?:|data:|blob:)/i.test(path)) return path;
    return `${this.base}${path.startsWith('/') ? '' : '/'}${path}`;
  }

  private url(path: string) { return `${this.base}/api/${path.replace(/^\//, '')}`; }

  private toParams(p?: Params): HttpParams {
    let params = new HttpParams();
    for (const [k, v] of Object.entries(p ?? {})) {
      if (v !== null && v !== undefined && v !== '') params = params.set(k, String(v));
    }
    return params;
  }
}

/** Human-readable message from any API error. */
export function errorMessage(e: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (e instanceof HttpErrorResponse) {
    if (e.status === 0) return 'Cannot reach the server. Check your connection and try again.';
    if (e.status === 429) return 'Too many attempts. Wait a minute and try again.';
    const body = e.error as ApiResponse<unknown> | { title?: string; errors?: Record<string, string[]> } | null;
    if (body && typeof body === 'object') {
      const b = body as any;
      if (Array.isArray(b.errors) && b.errors.length) return b.errors.join(' ');
      if (b.errors && typeof b.errors === 'object') return Object.values(b.errors).flat().join(' ');
      if (b.message) return b.message;
      if (b.title) return b.title;
    }
  }
  return fallback;
}

export const PLACEHOLDER =
  'data:image/svg+xml;utf8,' +
  encodeURIComponent(
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 400"><rect width="400" height="400" fill="#e3e6ff"/><g fill="none" stroke="#2d3df0" stroke-width="10" stroke-linecap="round" stroke-linejoin="round"><path d="M120 280V150l80 90 80-90v130"/></g></svg>`,
  );
