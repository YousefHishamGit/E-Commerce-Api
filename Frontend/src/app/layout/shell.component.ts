import { ChangeDetectionStrategy, Component, HostListener, computed, effect, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { CartService } from '../core/cart.service';
import { WishlistService } from '../core/wishlist.service';
import { IconComponent } from '../shared/icon.component';
import { LogoComponent } from './logo.component';
import { EgpPipe } from '../shared/egp.pipe';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [EgpPipe, RouterOutlet, RouterLink, RouterLinkActive, IconComponent, LogoComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a href="#main" class="sr-only z-[100] rounded-full bg-ink px-4 py-2 text-white focus:not-sr-only focus:fixed focus:left-4 focus:top-4">Skip to content</a>

    <!-- Promo strip -->
    <div class="bg-ink text-center text-sm font-medium text-white">
      <p class="container-x py-2">Free delivery on orders over <strong class="text-saffron">EGP 1,000</strong><span class="hidden sm:inline"> — pay by card, wallet or cash.</span></p>
    </div>

    <header class="sticky top-0 z-40 border-b-2 border-ink bg-paper/95 backdrop-blur">
      <div class="container-x flex h-[68px] items-center gap-3 lg:gap-6">
        <button class="btn btn-flat btn-sm lg:hidden" (click)="menuOpen.set(true)" aria-label="Open menu"><app-icon name="menu" size="1.4rem" /></button>
        <app-logo />

        <nav class="hidden items-center gap-1 lg:flex" aria-label="Main">
          <a routerLink="/" [routerLinkActiveOptions]="{exact:true}" routerLinkActive="bg-ink text-white" class="rounded-full px-4 py-1.5 font-display font-semibold hover:bg-ink/5">Home</a>
          <a routerLink="/shop" routerLinkActive="bg-ink text-white" class="rounded-full px-4 py-1.5 font-display font-semibold hover:bg-ink/5">Shop</a>
          <a routerLink="/shop" [queryParams]="{ sort: 'rating' }" class="rounded-full px-4 py-1.5 font-display font-semibold hover:bg-ink/5">Top rated</a>
        </nav>

        <form class="relative ml-auto hidden max-w-md flex-1 md:block" (submit)="$event.preventDefault(); search()" role="search">
          <app-icon name="search" size="1.1rem" class="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-ink-soft" />
          <input class="field !rounded-full !py-2 pl-11" type="search" placeholder="Search for something you like" [value]="q()" (input)="q.set($any($event.target).value)" aria-label="Search products" />
        </form>

        <div class="ml-auto flex items-center gap-1 md:ml-0">
          <button class="btn btn-flat btn-sm md:hidden" (click)="mobileSearch.set(!mobileSearch())" aria-label="Search"><app-icon name="search" size="1.3rem" /></button>

          <a routerLink="/wishlist" class="btn btn-flat btn-sm relative" aria-label="Favourites">
            <app-icon name="heart" size="1.3rem" />
            @if (wishlist.count() > 0) { <span class="absolute -right-0.5 -top-0.5 grid min-w-5 place-items-center rounded-full border-2 border-ink bg-coral px-1 text-[11px] font-bold leading-4 text-white">{{ wishlist.count() }}</span> }
          </a>

          <button class="btn btn-flat btn-sm relative" (click)="cart.drawerOpen.set(true)" aria-label="Open basket">
            <app-icon name="cart" size="1.3rem" />
            @if (cart.count() > 0) { <span class="absolute -right-0.5 -top-0.5 grid min-w-5 place-items-center rounded-full border-2 border-ink bg-saffron px-1 text-[11px] font-bold leading-4">{{ cart.count() }}</span> }
          </button>

          @if (auth.user(); as u) {
            <div class="relative">
              <button class="btn btn-light btn-sm ml-1" (click)="userMenu.set(!userMenu())" [attr.aria-expanded]="userMenu()" aria-haspopup="menu">
                <span class="grid size-6 place-items-center rounded-full bg-cobalt text-xs font-bold text-white">{{ u.fullName.charAt(0) }}</span>
                <span class="hidden sm:inline">{{ auth.firstName() }}</span>
                <app-icon name="chevronDown" size="0.9rem" />
              </button>
              @if (userMenu()) {
                <div class="card absolute right-0 top-full z-50 mt-2 w-56 animate-pop-in overflow-hidden p-1.5 !shadow-pop-sm" role="menu">
                  <p class="truncate px-3 py-2 text-xs text-ink-soft">{{ u.email }}</p>
                  @if (auth.isAdmin()) { <a routerLink="/admin" role="menuitem" class="menu-item"><app-icon name="chart" size="1.1rem" /> Admin dashboard</a> }
                  <a routerLink="/account" role="menuitem" class="menu-item"><app-icon name="user" size="1.1rem" /> My account</a>
                  <a routerLink="/orders" role="menuitem" class="menu-item"><app-icon name="package" size="1.1rem" /> My orders</a>
                  <a routerLink="/wishlist" role="menuitem" class="menu-item"><app-icon name="heart" size="1.1rem" /> Favourites</a>
                  <button role="menuitem" class="menu-item w-full text-coral" (click)="auth.logout()"><app-icon name="logout" size="1.1rem" /> Sign out</button>
                </div>
              }
            </div>
          } @else {
            <a routerLink="/login" class="btn btn-light btn-sm ml-1">Sign in</a>
            <a routerLink="/register" class="btn btn-accent btn-sm hidden sm:inline-flex">Join</a>
          }
        </div>
      </div>

      @if (mobileSearch()) {
        <form class="container-x pb-3 md:hidden" (submit)="$event.preventDefault(); search()" role="search">
          <input class="field !rounded-full" type="search" placeholder="Search for something you like" [value]="q()" (input)="q.set($any($event.target).value)" aria-label="Search products" />
        </form>
      }
    </header>

    <main id="main" class="min-h-[70vh]"><router-outlet /></main>

    <!-- Footer -->
    <footer class="mt-24 border-t-2 border-ink bg-ink text-white">
      <div class="container-x grid gap-10 py-14 md:grid-cols-[1.4fr_1fr_1fr_1fr]">
        <div class="max-w-xs">
          <app-logo [light]="true" />
          <p class="mt-4 text-white/70">Everyday things for every mood. Packed in Cairo, delivered across Egypt.</p>
        </div>
        <div>
          <h3 class="mb-3 text-lg font-bold">Shop</h3>
          <ul class="space-y-2 text-white/75">
            <li><a routerLink="/shop" class="hover:text-saffron">All products</a></li>
            @for (c of categories(); track c.id) { <li><a [routerLink]="['/category', c.id]" class="hover:text-saffron">{{ c.name }}</a></li> }
          </ul>
        </div>
        <div>
          <h3 class="mb-3 text-lg font-bold">Your account</h3>
          <ul class="space-y-2 text-white/75">
            <li><a routerLink="/account" class="hover:text-saffron">Profile & addresses</a></li>
            <li><a routerLink="/orders" class="hover:text-saffron">Track an order</a></li>
            <li><a routerLink="/wishlist" class="hover:text-saffron">Favourites</a></li>
          </ul>
        </div>
        <div>
          <h3 class="mb-3 text-lg font-bold">Good to know</h3>
          <ul class="space-y-3 text-white/75">
            <li class="flex gap-2"><app-icon name="truck" /> Delivery in 2–4 days</li>
            <li class="flex gap-2"><app-icon name="shield" /> Secure payments by Paymob</li>
            <li class="flex gap-2"><app-icon name="refresh" /> Cancel while an order is pending</li>
          </ul>
        </div>
      </div>
      <div class="border-t border-white/15 py-5 text-center text-sm text-white/55">© {{ year }} Mazaj. All rights reserved.</div>
    </footer>

    <!-- Cart drawer -->
    @if (cart.drawerOpen()) {
      <div class="fixed inset-0 z-50 flex justify-end" role="dialog" aria-modal="true" aria-label="Basket">
        <button class="absolute inset-0 bg-ink/50" (click)="cart.drawerOpen.set(false)" aria-label="Close basket"></button>
        <aside class="relative flex h-full w-full max-w-md animate-slide-in flex-col border-l-2 border-ink bg-paper">
          <div class="flex items-center justify-between border-b-2 border-ink px-5 py-4">
            <h2 class="text-2xl font-bold">Your basket <span class="text-ink-soft">({{ cart.count() }})</span></h2>
            <button class="btn btn-flat btn-sm" (click)="cart.drawerOpen.set(false)" aria-label="Close"><app-icon name="close" size="1.3rem" /></button>
          </div>

          @if (cart.items().length === 0) {
            <div class="grid flex-1 place-items-center p-8 text-center">
              <div>
                <span class="mx-auto grid size-16 place-items-center rounded-2xl border-2 border-ink bg-saffron shadow-pop-sm"><app-icon name="cart" size="1.75rem" /></span>
                <p class="mt-4 text-xl font-bold font-display">Nothing here yet</p>
                <p class="mt-1 text-ink-soft">Pick a few things you like and they will show up here.</p>
                <a routerLink="/shop" class="btn btn-primary mt-5" (click)="cart.drawerOpen.set(false)">Start shopping</a>
              </div>
            </div>
          } @else {
            <ul class="flex-1 divide-y divide-line overflow-y-auto px-5">
              @for (i of cart.items(); track i.id) {
                <li class="flex gap-3 py-4">
                  <a [routerLink]="['/product', i.id]" (click)="cart.drawerOpen.set(false)" class="size-20 shrink-0 overflow-hidden rounded-xl border-2 border-ink">
                    <img [src]="api.img(i.pictureUrl)" [alt]="i.productName" class="size-full object-cover" />
                  </a>
                  <div class="flex min-w-0 flex-1 flex-col">
                    <p class="line-clamp-2 font-display font-semibold leading-snug">{{ i.productName }}</p>
                    <p class="text-sm text-ink-soft">{{ i.price | egp:'1.0-2' }}</p>
                    <div class="mt-auto flex items-center justify-between pt-2">
                      <div class="inline-flex items-center rounded-full border-2 border-ink bg-white">
                        <button class="grid size-8 place-items-center" (click)="cart.setQuantity(i.id, i.quantity - 1)" aria-label="Decrease quantity"><app-icon name="minus" size="0.9rem" /></button>
                        <span class="w-7 text-center text-sm font-bold">{{ i.quantity }}</span>
                        <button class="grid size-8 place-items-center" (click)="cart.setQuantity(i.id, i.quantity + 1)" aria-label="Increase quantity"><app-icon name="plus" size="0.9rem" /></button>
                      </div>
                      <button class="text-ink-soft hover:text-coral" (click)="cart.remove(i.id)" aria-label="Remove item"><app-icon name="trash" size="1.1rem" /></button>
                    </div>
                  </div>
                </li>
              }
            </ul>
            <div class="space-y-3 border-t-2 border-ink bg-white p-5">
              <div class="flex items-baseline justify-between"><span class="text-ink-soft">Subtotal</span><span class="font-display text-2xl font-bold">{{ cart.subtotal() | egp:'1.0-2' }}</span></div>
              <p class="text-sm text-ink-soft">Shipping, tax and coupons are calculated at checkout.</p>
              <a routerLink="/checkout" class="btn btn-primary btn-lg w-full" (click)="cart.drawerOpen.set(false)">Checkout</a>
              <a routerLink="/cart" class="btn btn-light w-full" (click)="cart.drawerOpen.set(false)">View full basket</a>
            </div>
          }
        </aside>
      </div>
    }

    <!-- Mobile menu -->
    @if (menuOpen()) {
      <div class="fixed inset-0 z-50 flex" role="dialog" aria-modal="true" aria-label="Menu">
        <aside class="relative flex h-full w-72 max-w-[85%] flex-col gap-1 border-r-2 border-ink bg-paper p-5">
          <div class="mb-4 flex items-center justify-between"><app-logo /><button class="btn btn-flat btn-sm" (click)="menuOpen.set(false)" aria-label="Close menu"><app-icon name="close" size="1.3rem" /></button></div>
          <a routerLink="/" class="menu-item !text-lg" (click)="menuOpen.set(false)"><app-icon name="home" /> Home</a>
          <a routerLink="/shop" class="menu-item !text-lg" (click)="menuOpen.set(false)"><app-icon name="grid" /> Shop</a>
          @for (c of categories(); track c.id) { <a [routerLink]="['/category', c.id]" class="menu-item ml-6" (click)="menuOpen.set(false)">{{ c.name }}</a> }
          <a routerLink="/wishlist" class="menu-item !text-lg" (click)="menuOpen.set(false)"><app-icon name="heart" /> Favourites</a>
          <a routerLink="/orders" class="menu-item !text-lg" (click)="menuOpen.set(false)"><app-icon name="package" /> Orders</a>
        </aside>
        <button class="flex-1 bg-ink/50" (click)="menuOpen.set(false)" aria-label="Close menu"></button>
      </div>
    }
  `,
  styles: [`
    .menu-item { display:flex; align-items:center; gap:.6rem; border-radius:.75rem; padding:.55rem .75rem; font-weight:600; text-align:left; }
    .menu-item:hover { background: rgb(22 20 46 / .07); }
  `],
})
export class ShellComponent {
  auth = inject(AuthService);
  cart = inject(CartService);
  wishlist = inject(WishlistService);
  api = inject(ApiService);
  private router = inject(Router);

  year = new Date().getFullYear();
  q = signal('');
  menuOpen = signal(false);
  userMenu = signal(false);
  mobileSearch = signal(false);
  categories = signal<{ id: number; name: string }[]>([]);

  constructor() {
    this.api.get<{ id: number; name: string }[]>('Categories').subscribe({ next: c => this.categories.set(c.slice(0, 8)), error: () => {} });
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => {
      this.userMenu.set(false); this.menuOpen.set(false); this.mobileSearch.set(false); this.cart.drawerOpen.set(false);
      window.scrollTo({ top: 0 });
    });
  }

  search() {
    const term = this.q().trim();
    this.router.navigate(['/shop'], { queryParams: term ? { search: term } : {} });
  }

  @HostListener('document:keydown.escape')
  onEsc() { this.userMenu.set(false); this.menuOpen.set(false); this.cart.drawerOpen.set(false); }

  @HostListener('document:click', ['$event'])
  onDocClick(e: Event) {
    if (this.userMenu() && !(e.target as HTMLElement).closest('[aria-haspopup="menu"], [role="menu"]')) this.userMenu.set(false);
  }
}
