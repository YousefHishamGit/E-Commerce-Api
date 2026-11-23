import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { DashboardSummary, LowStockProduct, OrderStatusCount, RecentOrder, SalesPoint, TopProduct } from '../../core/models';
import { STATUS_STYLE } from '../../core/utils';
import { IconComponent } from '../../shared/icon.component';
import { EgpPipe } from '../../shared/egp.pipe';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [EgpPipe, DatePipe, RouterLink, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="mb-8 flex flex-wrap items-end justify-between gap-3">
      <div><h1 class="text-4xl font-extrabold">Dashboard</h1><p class="text-ink-soft">How the shop is doing right now.</p></div>
      <div class="flex items-center gap-2"><label for="days" class="text-sm font-semibold">Sales range</label>
        <select id="days" class="field !w-auto !py-1.5" [value]="days()" (change)="setDays(+$any($event.target).value)">
          <option value="7">7 days</option><option value="30">30 days</option><option value="90">90 days</option>
        </select>
      </div>
    </header>

    @if (loading()) { <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">@for (i of [1,2,3,4]; track i) { <div class="skeleton h-32 rounded-3xl"></div> }</div> }
    @else if (s()) {
      <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        @for (t of tiles(); track t.label) {
          <div class="card p-5" [class]="t.bg">
            <div class="flex items-center justify-between"><span class="font-semibold">{{ t.label }}</span><app-icon [name]="t.icon" /></div>
            <p class="mt-3 font-display text-4xl font-extrabold">{{ t.value }}</p>
            <p class="mt-1 text-sm opacity-80">{{ t.sub }}</p>
          </div>
        }
      </div>

      <div class="mt-8 grid gap-6 xl:grid-cols-[1fr_340px]">
        <section class="card p-6">
          <div class="mb-4 flex items-baseline justify-between"><h2 class="text-2xl font-bold">Revenue</h2><p class="text-sm text-ink-soft">Last {{ days() }} days · {{ chartTotal() | egp:'1.0-0' }}</p></div>
          @if (sales().length) {
            <svg viewBox="0 0 600 220" class="w-full" role="img" [attr.aria-label]="'Revenue over the last ' + days() + ' days'">
              @for (g of [0, 0.5, 1]; track g) { <line x1="0" x2="600" [attr.y1]="190 - g * 170" [attr.y2]="190 - g * 170" stroke="#dcdcec" stroke-width="1" /> }
              @for (b of bars(); track b.date) {
                <rect [attr.x]="b.x" [attr.y]="b.y" [attr.width]="b.w" [attr.height]="b.h" rx="3" fill="#2d3df0" stroke="#16142e" stroke-width="1.5"><title>{{ b.date | date:'d MMM' }}: {{ b.revenue | egp:'1.0-0' }}</title></rect>
              }
              <text x="0" y="212" font-size="11" fill="#4a4868">{{ sales()[0].date | date:'d MMM' }}</text>
              <text x="600" y="212" font-size="11" fill="#4a4868" text-anchor="end">{{ sales()[sales().length - 1].date | date:'d MMM' }}</text>
            </svg>
          } @else { <p class="py-10 text-center text-ink-soft">No sales in this period yet.</p> }
        </section>

        <section class="card p-6">
          <h2 class="mb-4 text-2xl font-bold">Orders by status</h2>
          <ul class="space-y-3">
            @for (o of statuses(); track o.status) {
              <li>
                <div class="mb-1 flex justify-between text-sm font-semibold"><span>{{ o.status }}</span><span>{{ o.count }}</span></div>
                <div class="h-3 overflow-hidden rounded-full border-2 border-ink bg-white"><div class="h-full" [class]="barColor(o.status)" [style.width.%]="statusPct(o.count)"></div></div>
              </li>
            } @empty { <li class="text-ink-soft">No orders yet.</li> }
          </ul>
        </section>
      </div>

      <div class="mt-6 grid gap-6 xl:grid-cols-3">
        <section class="card p-6 xl:col-span-2">
          <div class="mb-3 flex items-center justify-between"><h2 class="text-2xl font-bold">Recent orders</h2><a routerLink="/admin/orders" class="text-sm font-semibold text-cobalt hover:underline">All orders</a></div>
          <div class="overflow-x-auto">
            <table class="table-x">
              <thead><tr><th>Order</th><th>Customer</th><th>Status</th><th class="text-right">Total</th></tr></thead>
              <tbody>
                @for (o of recent(); track o.orderId) {
                  <tr><td><a [routerLink]="['/orders', o.orderId]" class="font-semibold hover:text-cobalt">{{ o.orderId.slice(0, 8) }}</a><div class="text-xs text-ink-soft">{{ o.createdAt | date:'d MMM, h:mm a' }}</div></td>
                    <td>{{ o.customerName }}</td><td><span class="badge" [class]="style(o.status)">{{ o.status }}</span></td>
                    <td class="text-right font-semibold">{{ o.totalPrice | egp:'1.0-0' }}</td></tr>
                } @empty { <tr><td colspan="4" class="py-6 text-center text-ink-soft">No orders yet.</td></tr> }
              </tbody>
            </table>
          </div>
        </section>

        <div class="space-y-6">
          <section class="card p-6">
            <h2 class="mb-3 text-2xl font-bold">Best sellers</h2>
            <ol class="space-y-3">
              @for (p of top(); track p.productId) {
                <li class="flex items-center gap-3"><img [src]="api.img(p.photo)" alt="" class="size-11 rounded-lg border-2 border-ink object-cover" />
                  <div class="min-w-0 flex-1"><p class="truncate font-semibold">{{ p.name }}</p><p class="text-xs text-ink-soft">{{ p.quantitySold }} sold</p></div>
                  <span class="font-display font-bold">{{ p.revenue | egp:'1.0-0' }}</span></li>
              } @empty { <li class="text-ink-soft">Nothing sold yet.</li> }
            </ol>
          </section>
          <section class="card p-6">
            <h2 class="mb-3 flex items-center gap-2 text-2xl font-bold"><app-icon name="alert" /> Low stock</h2>
            <ul class="space-y-2.5">
              @for (p of low(); track p.productId) {
                <li class="flex items-center justify-between gap-3"><a [routerLink]="['/admin/products', p.productId]" class="truncate font-semibold hover:text-cobalt">{{ p.name }}</a>
                  <span class="badge" [class]="p.stockQuantity === 0 ? 'bg-coral text-white' : 'bg-saffron'">{{ p.stockQuantity }} left</span></li>
              } @empty { <li class="text-ink-soft">Every product is well stocked.</li> }
            </ul>
          </section>
        </div>
      </div>
    }
  `,
})
export class AdminDashboardComponent {
  api = inject(ApiService);
  loading = signal(true);
  days = signal(30);
  s = signal<DashboardSummary | null>(null);
  sales = signal<SalesPoint[]>([]);
  top = signal<TopProduct[]>([]);
  low = signal<LowStockProduct[]>([]);
  statuses = signal<OrderStatusCount[]>([]);
  recent = signal<RecentOrder[]>([]);
  style = (s: string) => STATUS_STYLE[s] ?? '';

  private money = (n: number) => `EGP ${new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n)}`;
  tiles = computed(() => {
    const s = this.s(); if (!s) return [];
    return [
      { label: 'Revenue', value: this.money(s.totalRevenue), sub: `Today ${this.money(s.todayRevenue)}`, icon: 'wallet', bg: 'bg-saffron' },
      { label: 'Orders', value: String(s.totalOrders), sub: `${s.pendingOrders} pending · ${s.todayOrders} today`, icon: 'truck', bg: 'bg-cobalt text-white' },
      { label: 'Customers', value: String(s.totalCustomers), sub: `${s.newCustomersLast30Days} new in 30 days`, icon: 'users', bg: 'bg-mint' },
      { label: 'Products', value: String(s.totalProducts), sub: `${s.lowStockProducts} low · ${s.outOfStockProducts} sold out`, icon: 'package', bg: 'bg-white' },
    ];
  });
  chartTotal = computed(() => this.sales().reduce((n, p) => n + p.revenue, 0));
  bars = computed(() => {
    const pts = this.sales(); if (!pts.length) return [];
    const max = Math.max(...pts.map(p => p.revenue), 1);
    const slot = 600 / pts.length, w = Math.max(3, slot * 0.7);
    return pts.map((p, i) => { const h = Math.max(2, (p.revenue / max) * 170); return { date: p.date, revenue: p.revenue, x: i * slot + (slot - w) / 2, w, h, y: 190 - h }; });
  });
  private totalStatus = computed(() => Math.max(1, this.statuses().reduce((n, o) => n + o.count, 0)));
  statusPct = (n: number) => (n / this.totalStatus()) * 100;
  barColor = (s: string) => ({ Pending: 'bg-saffron', Confirmed: 'bg-cobalt', Shipped: 'bg-lilac', Delivered: 'bg-mint', Cancelled: 'bg-coral' } as Record<string, string>)[s] ?? 'bg-ink';

  constructor() { this.load(); }
  setDays(d: number) { this.days.set(d); this.api.get<SalesPoint[]>('Dashboard/sales', { days: d }).subscribe(s => this.sales.set(s)); }

  private load() {
    forkJoin({
      s: this.api.get<DashboardSummary>('Dashboard/summary'),
      sales: this.api.get<SalesPoint[]>('Dashboard/sales', { days: this.days() }),
      top: this.api.get<TopProduct[]>('Dashboard/top-products', { count: 5 }),
      low: this.api.get<LowStockProduct[]>('Dashboard/low-stock', { count: 6 }),
      statuses: this.api.get<OrderStatusCount[]>('Dashboard/orders-by-status'),
      recent: this.api.get<RecentOrder[]>('Dashboard/recent-orders', { count: 7 }),
    }).subscribe({
      next: r => { this.s.set(r.s); this.sales.set(r.sales); this.top.set(r.top); this.low.set(r.low); this.statuses.set(r.statuses); this.recent.set(r.recent); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
