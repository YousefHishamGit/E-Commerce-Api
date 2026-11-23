import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { CartService } from '../core/cart.service';
import { ToastService } from '../core/toast.service';
import { IconComponent } from '../shared/icon.component';
import { EmptyComponent } from '../shared/ui.components';
import { EgpPipe } from '../shared/egp.pipe';

/** Order summary lines, shared by the basket and checkout pages. */
@Component({
  selector: 'app-order-summary',
  standalone: true,
  imports: [EgpPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @let b = cart.basket();
    <dl class="space-y-2.5 text-[15px]">
      <div class="flex justify-between"><dt class="text-ink-soft">Subtotal</dt><dd class="font-semibold">{{ cart.subtotal() | egp:'1.2-2' }}</dd></div>
      @if (b?.offerDiscount) { <div class="flex justify-between"><dt class="text-ink-soft">Offer savings</dt><dd class="font-semibold text-mint">−{{ b!.offerDiscount | egp:'1.2-2' }}</dd></div> }
      @if (b?.couponDiscount) { <div class="flex justify-between"><dt class="text-ink-soft">Coupon {{ b!.couponCode }}</dt><dd class="font-semibold text-mint">−{{ b!.couponDiscount | egp:'1.2-2' }}</dd></div> }
      <div class="flex justify-between"><dt class="text-ink-soft">Shipping</dt><dd class="font-semibold">{{ b?.shippingCost == null ? '—' : b!.shippingCost === 0 ? 'Free' : (b!.shippingCost | egp:'1.2-2') }}</dd></div>
      <div class="flex justify-between"><dt class="text-ink-soft">Tax (14%)</dt><dd class="font-semibold">{{ b?.taxesCost == null ? '—' : (b!.taxesCost | egp:'1.2-2') }}</dd></div>
      <div class="flex items-baseline justify-between border-t-2 border-ink pt-3"><dt class="font-display text-lg font-bold">Total</dt><dd class="font-display text-2xl font-extrabold">{{ (b?.totalPrice ?? cart.subtotal()) | egp:'1.2-2' }}</dd></div>
    </dl>`,
})
export class OrderSummaryComponent { cart = inject(CartService); }

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [EgpPipe, RouterLink, FormsModule, IconComponent, EmptyComponent, OrderSummaryComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container-x pt-8 lg:pt-12">
      <h1 class="page-title">Your basket</h1>

      @if (cart.loading() && !cart.basket()) {
        <div class="skeleton mt-8 h-64 rounded-3xl"></div>
      } @else if (cart.items().length === 0) {
        <div class="mt-8"><app-empty icon="cart" title="Your basket is empty" text="Add a few things you like and come back here to check out." cta="Start shopping" link="/shop" /></div>
      } @else {
        <div class="mt-8 grid items-start gap-8 lg:grid-cols-[1fr_380px]">
          <ul class="card divide-y-2 divide-line overflow-hidden p-0">
            @for (i of cart.items(); track i.id) {
              <li class="flex gap-4 p-4 sm:p-5">
                <a [routerLink]="['/product', i.id]" class="size-24 shrink-0 overflow-hidden rounded-2xl border-2 border-ink sm:size-28"><img [src]="api.img(i.pictureUrl)" [alt]="i.productName" class="size-full object-cover" /></a>
                <div class="flex min-w-0 flex-1 flex-col">
                  <div class="flex justify-between gap-3">
                    <a [routerLink]="['/product', i.id]" class="font-display text-lg font-semibold leading-snug hover:text-cobalt">{{ i.productName }}</a>
                    <p class="shrink-0 font-display text-lg font-bold">{{ i.price * i.quantity | egp:'1.0-2' }}</p>
                  </div>
                  <p class="text-sm text-ink-soft">{{ i.price | egp:'1.0-2' }} each</p>
                  <div class="mt-auto flex items-center justify-between pt-3">
                    <div class="inline-flex items-center rounded-full border-2 border-ink bg-white">
                      <button class="grid size-9 place-items-center" (click)="cart.setQuantity(i.id, i.quantity - 1)" aria-label="Decrease quantity"><app-icon name="minus" size="0.9rem" /></button>
                      <span class="w-8 text-center font-bold">{{ i.quantity }}</span>
                      <button class="grid size-9 place-items-center" (click)="cart.setQuantity(i.id, i.quantity + 1)" aria-label="Increase quantity"><app-icon name="plus" size="0.9rem" /></button>
                    </div>
                    <button class="inline-flex items-center gap-1.5 text-sm font-semibold text-ink-soft hover:text-coral" (click)="cart.remove(i.id)"><app-icon name="trash" size="1rem" /> Remove</button>
                  </div>
                </div>
              </li>
            }
          </ul>

          <aside class="card space-y-5 p-6 lg:sticky lg:top-24">
            <h2 class="text-2xl font-bold">Summary</h2>

            <div>
              @if (cart.basket()?.couponCode; as code) {
                <div class="flex items-center justify-between rounded-xl border-2 border-dashed border-mint bg-mint-soft px-3 py-2.5">
                  <span class="flex items-center gap-2 font-semibold"><app-icon name="ticket" size="1.1rem" /> {{ code }} applied</span>
                  <button class="text-sm font-semibold underline" (click)="removeCoupon()">Remove</button>
                </div>
              } @else {
                <form class="flex gap-2" (submit)="$event.preventDefault(); applyCoupon()">
                  <input class="field !py-2 uppercase" placeholder="Coupon code" [(ngModel)]="code" name="code" aria-label="Coupon code" maxlength="50" />
                  <button class="btn btn-light btn-sm" [disabled]="!code.trim() || applying()">Apply</button>
                </form>
                @if (couponError()) { <p class="hint-error" role="alert">{{ couponError() }}</p> }
              }
            </div>

            <app-order-summary />
            <a routerLink="/checkout" class="btn btn-primary btn-lg w-full">Go to checkout</a>
            <button class="btn btn-flat btn-sm w-full text-ink-soft" (click)="clear()">Empty basket</button>
          </aside>
        </div>
      }
    </section>
  `,
})
export class CartComponent {
  cart = inject(CartService);
  api = inject(ApiService);
  private toast = inject(ToastService);
  code = '';
  applying = signal(false);
  couponError = signal('');

  applyCoupon() {
    this.applying.set(true); this.couponError.set('');
    this.cart.applyCoupon(this.code.trim()).subscribe({
      next: () => { this.toast.success('Coupon applied'); this.code = ''; this.applying.set(false); },
      error: e => { this.couponError.set(errorMessage(e, 'That coupon is not valid.')); this.applying.set(false); },
    });
  }
  removeCoupon() { this.cart.removeCoupon().subscribe({ error: e => this.toast.error(errorMessage(e)) }); }
  clear() {
    this.api.delete<string>('Basket').subscribe({ next: () => { this.cart.clearLocal(); this.cart.load(); }, error: e => this.toast.error(errorMessage(e)) });
  }
}
