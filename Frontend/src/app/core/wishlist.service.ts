import { Injectable, computed, effect, inject, signal, untracked } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService, errorMessage } from './api.service';
import { AuthService } from './auth.service';
import { Wishlist } from './models';
import { ToastService } from './toast.service';

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private api = inject(ApiService);
  private auth = inject(AuthService);
  private toast = inject(ToastService);
  private router = inject(Router);

  readonly wishlist = signal<Wishlist | null>(null);
  readonly items = computed(() => this.wishlist()?.items ?? []);
  readonly count = computed(() => this.items().length);
  private ids = computed(() => new Set(this.items().map(i => i.productId)));

  constructor() {
    effect(() => {
      const loggedIn = this.auth.isLoggedIn();
      untracked(() => (loggedIn ? this.load() : this.wishlist.set(null)));
    });
  }

  has(productId: number) { return this.ids().has(productId); }

  load() { this.api.get<Wishlist>('Wishlist').subscribe({ next: w => this.wishlist.set(w), error: () => {} }); }

  toggle(productId: number) {
    if (!this.auth.isLoggedIn()) {
      this.toast.info('Sign in to save favourites.');
      this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }
    const had = this.has(productId);
    const req = had ? this.api.delete<Wishlist>(`Wishlist/${productId}`) : this.api.post<Wishlist>('Wishlist', { productId });
    req.subscribe({
      next: w => { this.wishlist.set(w); this.toast.success(had ? 'Removed from favourites' : 'Saved to favourites'); },
      error: e => this.toast.error(errorMessage(e)),
    });
  }
}
