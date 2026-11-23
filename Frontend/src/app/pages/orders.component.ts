import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { Order, PayOrderResponse } from '../core/models';
import { ToastService } from '../core/toast.service';
import { PAYMENT_LABEL, STATUS_STYLE } from '../core/utils';
import { IconComponent } from '../shared/icon.component';
import { EmptyComponent } from '../shared/ui.components';
import { EgpPipe } from '../shared/egp.pipe';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [EgpPipe, RouterLink, DatePipe, IconComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container-x pt-8 lg:pt-12">
      <h1 class="page-title">Your orders</h1>
      @if (loading()) {
        <div class="mt-8 space-y-4"><div class="skeleton h-28 rounded-3xl"></div><div class="skeleton h-28 rounded-3xl"></div></div>
      } @else if (orders().length === 0) {
        <div class="mt-8"><app-empty icon="package" title="No orders yet" text="When you place an order it will show up here, with live status." cta="Start shopping" link="/shop" /></div>
      } @else {
        <ul class="mt-8 space-y-4">
          @for (o of orders(); track o.id) {
            <li>
              <a [routerLink]="['/orders', o.id]" class="card flex flex-wrap items-center gap-x-6 gap-y-3 p-5 transition-transform hover:-translate-y-0.5">
                <div class="flex -space-x-3">
                  @for (i of o.items.slice(0, 3); track i.id) { <img [src]="api.img(i.productPhoto)" alt="" class="size-14 rounded-xl border-2 border-ink bg-white object-cover" /> }
                </div>
                <div class="min-w-0 flex-1">
                  <p class="font-display text-lg font-bold">Order {{ o.id.slice(0, 8) }}</p>
                  <p class="text-sm text-ink-soft">{{ o.createdAt | date:'d MMM y, h:mm a' }} · {{ o.items.length }} {{ o.items.length === 1 ? 'item' : 'items' }}</p>
                </div>
                <span class="badge" [class]="style(o.status)">{{ o.status }}</span>
                <p class="font-display text-xl font-extrabold">{{ o.totalPrice | egp:'1.0-2' }}</p>
                <app-icon name="chevronRight" class="text-ink-soft" />
              </a>
            </li>
          }
        </ul>
      }
    </section>
  `,
})
export class OrdersComponent {
  api = inject(ApiService);
  orders = signal<Order[]>([]);
  loading = signal(true);
  style = (s: string) => STATUS_STYLE[s] ?? '';
  constructor() {
    this.api.get<Order[]>('Orders').subscribe({
      next: o => { this.orders.set([...o].sort((a, b) => +new Date(b.createdAt) - +new Date(a.createdAt))); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}

const STEPS = ['Pending', 'Confirmed', 'Shipped', 'Delivered'] as const;

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [RouterLink, EgpPipe, DatePipe, IconComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container-x pt-8 lg:pt-12">
      <a routerLink="/orders" class="mb-4 inline-flex items-center gap-1 text-sm font-semibold text-ink-soft hover:text-cobalt"><app-icon name="chevronLeft" size="1rem" /> All orders</a>

      @let o = order();
      @if (loading()) { <div class="skeleton h-96 rounded-3xl"></div> }
      @else if (o) {
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 class="page-title">Order {{ o.id.slice(0, 8) }}</h1>
            <p class="mt-1 text-ink-soft">Placed {{ o.createdAt | date:'d MMMM y, h:mm a' }}</p>
          </div>
          <span class="badge text-sm" [class]="style(o.status)">{{ o.status }}</span>
        </div>

        <!-- Progress -->
        @if (o.status !== 'Cancelled') {
          <ol class="card mt-8 grid grid-cols-4 gap-2 p-5 text-center" aria-label="Order progress">
            @for (s of steps; track s; let i = $index) {
              <li class="flex flex-col items-center gap-2">
                <span class="grid size-10 place-items-center rounded-full border-2 border-ink font-bold" [class]="i <= stepIndex() ? 'bg-mint' : 'bg-white'">
                  @if (i <= stepIndex()) { <app-icon name="check" size="1.1rem" [stroke]="3" /> } @else { {{ i + 1 }} }
                </span>
                <span class="text-xs font-semibold sm:text-sm" [class.text-ink-soft]="i > stepIndex()">{{ s }}</span>
              </li>
            }
          </ol>
        } @else {
          <div class="card mt-8 flex items-center gap-3 bg-coral-soft p-5"><app-icon name="alert" /> This order was cancelled.</div>
        }

        <div class="mt-8 grid items-start gap-8 lg:grid-cols-[1fr_360px]">
          <ul class="card divide-y-2 divide-line overflow-hidden p-0">
            @for (i of o.items; track i.id) {
              <li class="flex items-center gap-4 p-4">
                <a [routerLink]="['/product', i.productId]"><img [src]="api.img(i.productPhoto)" alt="" class="size-20 rounded-xl border-2 border-ink object-cover" /></a>
                <div class="min-w-0 flex-1">
                  <a [routerLink]="['/product', i.productId]" class="font-display font-semibold hover:text-cobalt">{{ i.productName }}</a>
                  <p class="text-sm text-ink-soft">{{ i.quantity }} × {{ i.unitPrice | egp:'1.0-2' }}</p>
                </div>
                <p class="font-display font-bold">{{ i.subTotal | egp:'1.0-2' }}</p>
              </li>
            }
          </ul>

          <aside class="space-y-5">
            <div class="card space-y-2 p-5 text-[15px]">
              <h2 class="mb-2 text-xl font-bold">Summary</h2>
              <div class="flex justify-between"><span class="text-ink-soft">Subtotal</span><span>{{ o.subTotal | egp:'1.2-2' }}</span></div>
              @if (o.discountAmount > 0) { <div class="flex justify-between"><span class="text-ink-soft">Discount {{ o.couponCode }}</span><span class="text-mint">−{{ o.discountAmount | egp:'1.2-2' }}</span></div> }
              <div class="flex justify-between"><span class="text-ink-soft">Shipping</span><span>{{ o.shippingCost === 0 ? 'Free' : (o.shippingCost | egp:'1.2-2') }}</span></div>
              <div class="flex justify-between"><span class="text-ink-soft">Tax</span><span>{{ o.taxAmount | egp:'1.2-2' }}</span></div>
              <div class="flex items-baseline justify-between border-t-2 border-ink pt-3"><span class="font-display font-bold">Total</span><span class="font-display text-2xl font-extrabold">{{ o.totalPrice | egp:'1.2-2' }}</span></div>
            </div>

            <div class="card-flat space-y-1 p-5">
              <h2 class="mb-1 flex items-center gap-2 text-lg font-bold"><app-icon name="pin" size="1.1rem" /> Delivery</h2>
              <p class="font-semibold">{{ o.shippingCity }}</p>
              <p class="text-ink-soft">{{ o.shippingStreet }}</p>
              @if (o.shippingLocationUrl) { <a [href]="o.shippingLocationUrl" target="_blank" rel="noopener noreferrer" class="text-sm font-semibold text-cobalt underline">Open in maps</a> }
            </div>

            <div class="card-flat space-y-2 p-5">
              <h2 class="mb-1 flex items-center gap-2 text-lg font-bold"><app-icon name="card" size="1.1rem" /> Payment</h2>
              @for (p of o.payments; track p.id) {
                <div class="flex items-center justify-between gap-2 text-sm"><span>{{ payLabel(p.method) }}</span><span class="badge" [class]="style(p.status)">{{ p.status }}</span></div>
              } @empty { <p class="text-sm text-ink-soft">No payment recorded yet.</p> }
              @if (canPay()) { <button class="btn btn-accent mt-2 w-full" [disabled]="paying()" (click)="pay()">{{ paying() ? 'Opening payment…' : 'Pay now' }}</button> }
            </div>

            @if (o.status === 'Pending') { <button class="btn btn-light w-full text-coral" [disabled]="cancelling()" (click)="cancel()">{{ cancelling() ? 'Cancelling…' : 'Cancel this order' }}</button> }
          </aside>
        </div>
      } @else {
        <app-empty icon="search" title="Order not found" text="This order does not exist or belongs to another account." cta="Your orders" link="/orders" />
      }
    </section>
  `,
})
export class OrderDetailComponent {
  api = inject(ApiService);
  private toast = inject(ToastService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  order = signal<Order | null>(null);
  loading = signal(true);
  paying = signal(false);
  cancelling = signal(false);
  steps = STEPS;
  stepIndex = computed(() => STEPS.indexOf((this.order()?.status ?? 'Pending') as (typeof STEPS)[number]));
  canPay = computed(() => {
    const o = this.order();
    if (!o || o.status !== 'Pending') return false;
    const p = o.payments[0];
    return !!p && p.method !== 'CashOnDelivery' && p.status !== 'Completed';
  });
  style = (s: string) => STATUS_STYLE[s] ?? '';
  payLabel = (m: string) => PAYMENT_LABEL[m] ?? m;

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(m => this.load(m.get('id')!));
  }

  private load(id: string) {
    this.api.get<Order>(`Orders/${id}`).subscribe({ next: o => { this.order.set(o); this.loading.set(false); }, error: () => { this.order.set(null); this.loading.set(false); } });
  }

  pay() {
    this.paying.set(true);
    this.api.post<PayOrderResponse>(`Payments/orders/${this.order()!.id}/pay`).subscribe({
      next: p => { window.location.href = p.checkoutUrl; },
      error: e => { this.toast.error(errorMessage(e)); this.paying.set(false); },
    });
  }

  cancel() {
    if (!confirm('Cancel this order? This cannot be undone.')) return;
    this.cancelling.set(true);
    this.api.post<Order>(`Orders/${this.order()!.id}/cancel`).subscribe({
      next: o => { this.order.set(o); this.cancelling.set(false); this.toast.success('Order cancelled'); },
      error: e => { this.toast.error(errorMessage(e)); this.cancelling.set(false); },
    });
  }
}

/** Landing page after Paymob redirects the customer back. The webhook may lag, so we poll briefly. */
@Component({
  selector: 'app-payment-result',
  standalone: true,
  imports: [RouterLink, EgpPipe, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container-x grid min-h-[60vh] place-items-center py-12">
      <div class="card max-w-lg p-8 text-center sm:p-10">
        @switch (state()) {
          @case ('checking') {
            <div class="mx-auto size-14 animate-spin rounded-full border-4 border-cobalt border-t-transparent"></div>
            <h1 class="mt-6 text-3xl font-bold">Confirming your payment…</h1>
            <p class="mt-2 text-ink-soft">This usually takes a few seconds. Please do not close this page.</p>
          }
          @case ('paid') {
            <span class="mx-auto grid size-16 place-items-center rounded-full border-2 border-ink bg-mint shadow-pop-sm"><app-icon name="check" size="2rem" [stroke]="3" /></span>
            <h1 class="mt-6 text-3xl font-bold">Payment received</h1>
            <p class="mt-2 text-ink-soft">Thank you! We got {{ order()?.totalPrice | egp:'1.2-2' }} and will start packing your order.</p>
          }
          @case ('failed') {
            <span class="mx-auto grid size-16 place-items-center rounded-full border-2 border-ink bg-coral text-white shadow-pop-sm"><app-icon name="close" size="2rem" [stroke]="3" /></span>
            <h1 class="mt-6 text-3xl font-bold">Payment did not go through</h1>
            <p class="mt-2 text-ink-soft">No money was taken. You can try again from your order page.</p>
          }
          @default {
            <span class="mx-auto grid size-16 place-items-center rounded-full border-2 border-ink bg-saffron shadow-pop-sm"><app-icon name="alert" size="2rem" /></span>
            <h1 class="mt-6 text-3xl font-bold">Still waiting for confirmation</h1>
            <p class="mt-2 text-ink-soft">Your bank has not confirmed yet. Check your order page in a few minutes.</p>
          }
        }
        <div class="mt-7 flex flex-wrap justify-center gap-3">
          <a [routerLink]="['/orders', id]" class="btn btn-primary">View order</a>
          <a routerLink="/shop" class="btn btn-light">Keep shopping</a>
        </div>
      </div>
    </section>
  `,
})
export class PaymentResultComponent {
  private api = inject(ApiService);
  id = inject(ActivatedRoute).snapshot.paramMap.get('id')!;
  order = signal<Order | null>(null);
  state = signal<'checking' | 'paid' | 'failed' | 'unknown'>('checking');
  private tries = 0;
  private timer?: ReturnType<typeof setTimeout>;

  constructor() {
    inject(DestroyRef).onDestroy(() => clearTimeout(this.timer));
    this.check();
  }

  private check() {
    this.api.get<Order>(`Orders/${this.id}`).subscribe({
      next: o => {
        this.order.set(o);
        const s = o.payments[0]?.status;
        if (s === 'Completed') return this.state.set('paid');
        if (s === 'Failed') return this.state.set('failed');
        if (++this.tries < 8) this.timer = setTimeout(() => this.check(), 2500);
        else this.state.set('unknown');
      },
      error: () => this.state.set('unknown'),
    });
  }
}
