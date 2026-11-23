import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { CartService } from '../core/cart.service';
import { Address, Order, PayOrderResponse, PaymentMethod } from '../core/models';
import { ToastService } from '../core/toast.service';
import { IconComponent } from '../shared/icon.component';
import { EmptyComponent } from '../shared/ui.components';
import { OrderSummaryComponent } from './cart.component';

interface Method { id: PaymentMethod; title: string; text: string; icon: string; online: boolean; }

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, IconComponent, EmptyComponent, OrderSummaryComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container-x pt-8 lg:pt-12">
      <h1 class="page-title">Checkout</h1>

      @if (cart.items().length === 0 && !cart.loading()) {
        <div class="mt-8"><app-empty icon="cart" title="Your basket is empty" text="Add something to your basket before checking out." cta="Browse the shop" link="/shop" /></div>
      } @else {
        <div class="mt-8 grid items-start gap-8 lg:grid-cols-[1fr_380px]">
          <div class="space-y-8">
            <!-- 1. Address -->
            <section class="card p-6">
              <div class="flex items-center justify-between">
                <h2 class="text-2xl font-bold">Where should we deliver?</h2>
                <button class="btn btn-light btn-sm" (click)="adding.set(!adding())"><app-icon [name]="adding() ? 'close' : 'plus'" size="1rem" /> {{ adding() ? 'Cancel' : 'New address' }}</button>
              </div>

              @if (adding() || addresses().length === 0) {
                <form class="mt-5 grid gap-4 sm:grid-cols-2" [formGroup]="addressForm" (ngSubmit)="saveAddress()">
                  <div>
                    <label class="label" for="city">City</label>
                    <input id="city" class="field" formControlName="city" placeholder="Cairo" autocomplete="address-level2" />
                    @if (addressForm.controls.city.touched && addressForm.controls.city.invalid) { <p class="hint-error">Enter your city (2+ letters).</p> }
                  </div>
                  <div>
                    <label class="label" for="street">Street and building</label>
                    <input id="street" class="field" formControlName="street" placeholder="12 Tahrir St, apt 4" autocomplete="street-address" />
                    @if (addressForm.controls.street.touched && addressForm.controls.street.invalid) { <p class="hint-error">Enter your street (3+ characters).</p> }
                  </div>
                  <div class="sm:col-span-2">
                    <label class="label" for="loc">Google Maps link (optional)</label>
                    <input id="loc" class="field" formControlName="locationUrl" placeholder="https://maps.google.com/…" />
                    @if (addressForm.controls.locationUrl.invalid) { <p class="hint-error">Paste a full link starting with https://</p> }
                  </div>
                  <div class="sm:col-span-2"><button class="btn btn-primary" [disabled]="addressForm.invalid || savingAddress()">{{ savingAddress() ? 'Saving…' : 'Save address' }}</button></div>
                </form>
              }

              @if (addresses().length) {
                <ul class="mt-5 grid gap-3 sm:grid-cols-2" role="radiogroup" aria-label="Delivery address">
                  @for (a of addresses(); track a.id) {
                    <li>
                      <button type="button" role="radio" [attr.aria-checked]="addressId() === a.id" (click)="addressId.set(a.id)"
                        class="flex w-full items-start gap-3 rounded-2xl border-2 p-4 text-left transition-all"
                        [class]="addressId() === a.id ? 'border-ink bg-cobalt-soft shadow-pop-sm' : 'border-line bg-white hover:border-ink'">
                        <app-icon name="pin" class="mt-0.5 text-cobalt" />
                        <span><span class="block font-display font-bold">{{ a.city }}</span><span class="block text-sm text-ink-soft">{{ a.street }}</span></span>
                      </button>
                    </li>
                  }
                </ul>
              }
            </section>

            <!-- 2. Payment -->
            <section class="card p-6">
              <h2 class="text-2xl font-bold">How would you like to pay?</h2>
              <ul class="mt-5 grid gap-3" role="radiogroup" aria-label="Payment method">
                @for (m of methods; track m.id) {
                  <li>
                    <button type="button" role="radio" [attr.aria-checked]="method() === m.id" (click)="method.set(m.id)"
                      class="flex w-full items-center gap-4 rounded-2xl border-2 p-4 text-left transition-all"
                      [class]="method() === m.id ? 'border-ink bg-saffron-soft shadow-pop-sm' : 'border-line bg-white hover:border-ink'">
                      <span class="grid size-11 shrink-0 place-items-center rounded-xl border-2 border-ink bg-white"><app-icon [name]="m.icon" /></span>
                      <span class="flex-1"><span class="block font-display font-bold">{{ m.title }}</span><span class="block text-sm text-ink-soft">{{ m.text }}</span></span>
                      <span class="grid size-6 place-items-center rounded-full border-2 border-ink" [class.bg-cobalt]="method() === m.id">
                        @if (method() === m.id) { <app-icon name="check" size="0.8rem" [stroke]="3.5" class="text-white" /> }
                      </span>
                    </button>
                  </li>
                }
              </ul>
            </section>
          </div>

          <aside class="card space-y-5 p-6 lg:sticky lg:top-24">
            <h2 class="text-2xl font-bold">Your order</h2>
            <ul class="max-h-48 space-y-2 overflow-y-auto text-sm">
              @for (i of cart.items(); track i.id) { <li class="flex justify-between gap-3"><span class="truncate">{{ i.quantity }} × {{ i.productName }}</span></li> }
            </ul>
            <app-order-summary />
            <button class="btn btn-primary btn-lg w-full" [disabled]="!addressId() || placing()" (click)="placeOrder()">
              {{ placing() ? 'Placing your order…' : isOnline() ? 'Continue to payment' : 'Place order' }}
            </button>
            @if (!addressId()) { <p class="text-center text-sm text-ink-soft">Choose or add a delivery address first.</p> }
            <a routerLink="/cart" class="block text-center text-sm font-semibold text-ink-soft underline">Back to basket</a>
          </aside>
        </div>
      }
    </section>
  `,
})
export class CheckoutComponent {
  cart = inject(CartService);
  private api = inject(ApiService);
  private toast = inject(ToastService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  addresses = signal<Address[]>([]);
  addressId = signal<number | null>(null);
  method = signal<PaymentMethod>('CashOnDelivery');
  adding = signal(false);
  savingAddress = signal(false);
  placing = signal(false);

  readonly methods: Method[] = [
    { id: 'CashOnDelivery', title: 'Cash on delivery', text: 'Pay in cash when your order arrives.', icon: 'cash', online: false },
    { id: 'CreditCard', title: 'Card', text: 'Visa or Mastercard through Paymob’s secure page.', icon: 'card', online: true },
    { id: 'Wallet', title: 'Mobile wallet', text: 'Vodafone Cash, Orange, Etisalat and other wallets.', icon: 'wallet', online: true },
  ];
  isOnline = computed(() => this.methods.find(m => m.id === this.method())?.online ?? false);

  addressForm = this.fb.nonNullable.group({
    city: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    street: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    locationUrl: ['', [Validators.pattern(/^$|^https?:\/\/.+/i), Validators.maxLength(500)]],
  });

  constructor() {
    this.api.get<Address[]>('Addresses').subscribe({
      next: a => { this.addresses.set(a); if (a.length) this.addressId.set(a[0].id); },
      error: () => {},
    });
  }

  saveAddress() {
    if (this.addressForm.invalid) { this.addressForm.markAllAsTouched(); return; }
    const v = this.addressForm.getRawValue();
    this.savingAddress.set(true);
    this.api.post<Address>('Addresses', { city: v.city.trim(), street: v.street.trim(), locationUrl: v.locationUrl.trim() || null }).subscribe({
      next: a => { this.addresses.update(l => [...l, a]); this.addressId.set(a.id); this.adding.set(false); this.addressForm.reset(); this.savingAddress.set(false); this.toast.success('Address saved'); },
      error: e => { this.toast.error(errorMessage(e)); this.savingAddress.set(false); },
    });
  }

  placeOrder() {
    const addressId = this.addressId();
    if (!addressId) return;
    this.placing.set(true);
    this.api.post<Order>('Orders', { deliveryAddressId: addressId, paymentMethod: this.method(), couponCode: this.cart.basket()?.couponCode ?? null }).subscribe({
      next: order => {
        this.cart.clearLocal();
        if (!this.isOnline()) { this.cart.load(); this.toast.success('Order placed. Thank you!'); this.router.navigate(['/orders', order.id]); return; }
        this.api.post<PayOrderResponse>(`Payments/orders/${order.id}/pay`).subscribe({
          next: p => { window.location.href = p.checkoutUrl; },
          error: e => {
            this.placing.set(false); this.cart.load();
            this.toast.error(errorMessage(e, 'We could not start the payment.') + ' You can retry from your order page.');
            this.router.navigate(['/orders', order.id]);
          },
        });
      },
      error: e => { this.toast.error(errorMessage(e)); this.placing.set(false); },
    });
  }
}
