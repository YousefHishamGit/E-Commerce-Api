import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth.service';
import { IconComponent } from '../shared/icon.component';
import { LogoComponent } from './logo.component';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, IconComponent, LogoComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="min-h-screen lg:grid lg:grid-cols-[250px_1fr]">
      <aside class="border-b-2 border-ink bg-ink text-white lg:sticky lg:top-0 lg:h-screen lg:border-b-0 lg:border-r-2">
        <div class="flex items-center justify-between gap-3 px-4 py-3 lg:flex-col lg:items-start lg:gap-8 lg:p-6">
          <div class="flex items-center gap-2"><app-logo [light]="true" /><span class="badge bg-saffron text-ink">Admin</span></div>
          <nav class="flex gap-1 overflow-x-auto lg:w-full lg:flex-col" aria-label="Admin">
            @for (l of links; track l.path) {
              <a [routerLink]="l.path" [routerLinkActiveOptions]="{ exact: l.path === '/admin' }" routerLinkActive="!bg-white !text-ink"
                 class="flex items-center gap-3 whitespace-nowrap rounded-xl px-3 py-2 font-display font-semibold text-white/80 transition-colors hover:bg-white/10">
                <app-icon [name]="l.icon" size="1.15rem" /> <span class="hidden sm:inline lg:inline">{{ l.label }}</span>
              </a>
            }
          </nav>
        </div>
        <div class="hidden space-y-2 p-6 pt-0 lg:absolute lg:bottom-0 lg:block lg:w-full">
          <a routerLink="/" class="flex items-center gap-2 text-sm text-white/70 hover:text-saffron"><app-icon name="eye" size="1rem" /> View storefront</a>
          <button class="flex items-center gap-2 text-sm text-white/70 hover:text-coral" (click)="auth.logout()"><app-icon name="logout" size="1rem" /> Sign out</button>
        </div>
      </aside>
      <main class="min-w-0 p-4 sm:p-6 lg:p-10"><router-outlet /></main>
    </div>
  `,
})
export class AdminShellComponent {
  auth = inject(AuthService);
  links = [
    { path: '/admin', label: 'Dashboard', icon: 'chart' },
    { path: '/admin/products', label: 'Products', icon: 'package' },
    { path: '/admin/categories', label: 'Categories', icon: 'layers' },
    { path: '/admin/orders', label: 'Orders', icon: 'truck' },
    { path: '/admin/coupons', label: 'Coupons', icon: 'ticket' },
    { path: '/admin/offers', label: 'Offers', icon: 'percent' },
  ];
}
