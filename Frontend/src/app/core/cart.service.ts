import { Injectable, computed, effect, inject, signal, untracked } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { ApiService, errorMessage } from './api.service';
import { AuthService } from './auth.service';
import { Basket } from './models';
import { ToastService } from './toast.service';

/** Server-side basket (Redis) for the signed-in user, exposed as signals. */
@Injectable({ providedIn: 'root' })
export class CartService {
  private api = inject(ApiService);
  private auth = inject(AuthService);
  private toast = inject(ToastService);
  private router = inject(Router);

  readonly basket = signal<Basket | null>(null);
  readonly loading = signal(false);
  readonly drawerOpen = signal(false);

  readonly items = computed(() => this.basket()?.items ?? []);
  readonly count = computed(() => this.items().reduce((n, i) => n + i.quantity, 0));
  readonly subtotal = computed(() => this.items().reduce((n, i) => n + i.price * i.quantity, 0));

  constructor() {
    effect(() => {
      const loggedIn = this.auth.isLoggedIn();
      untracked(() => (loggedIn ? this.load() : this.basket.set(null)));
    });
  }

  load() {
    this.loading.set(true);
    this.api.get<Basket>('Basket').subscribe({
      next: b => { this.basket.set(b); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  add(productId: number, quantity = 1, open = true) {
    if (!this.auth.isLoggedIn()) {
      this.toast.info('Sign in to start your basket.');
      this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }
    this.api.post<Basket>('Basket/items', { productId, quantity }).subscribe({
      next: b => { this.basket.set(b); this.toast.success('Added to basket'); if (open) this.drawerOpen.set(true); },
      error: e => this.toast.error(errorMessage(e)),
    });
  }

  setQuantity(itemId: number, quantity: number) {
    const req = quantity <= 0
      ? this.api.delete<Basket>(`Basket/items/${itemId}`)
      : this.api.put<Basket>(`Basket/items/${itemId}`, { quantity });
    req.subscribe({ next: b => this.basket.set(b), error: e => this.toast.error(errorMessage(e)) });
  }

  remove(itemId: number) { this.setQuantity(itemId, 0); }

  applyCoupon(code: string) {
    return this.api.post<Basket>('Basket/coupon', { code }).pipe(tap(b => this.basket.set(b)));
  }
  removeCoupon() {
    return this.api.delete<Basket>('Basket/coupon').pipe(tap(b => this.basket.set(b)));
  }
  clearLocal() { this.basket.set(null); }
}
