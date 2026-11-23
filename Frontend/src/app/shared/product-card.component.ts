import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { CartService } from '../core/cart.service';
import { Product } from '../core/models';
import { WishlistService } from '../core/wishlist.service';
import { moodFor } from '../core/utils';
import { IconComponent } from './icon.component';
import { PriceComponent, StarsComponent } from './ui.components';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [RouterLink, DecimalPipe, IconComponent, PriceComponent, StarsComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @let p = product();
    <article class="group card-flat relative flex h-full flex-col overflow-hidden p-3 transition-all duration-200 hover:-translate-y-1 hover:border-ink hover:shadow-pop">
      <a [routerLink]="['/product', p.id]" class="relative block overflow-hidden rounded-xl" [class]="mood().soft" [attr.aria-label]="p.name">
        <img [src]="api.img(p.photo)" [alt]="p.name" loading="lazy" class="aspect-square w-full object-cover transition-transform duration-300 group-hover:scale-105" [class.opacity-60]="outOfStock()" />
        @if (p.hasOffer && p.discountPercentage) {
          <span class="badge absolute left-2 top-2 bg-coral text-white">−{{ p.discountPercentage | number:'1.0-0' }}%</span>
        }
        @if (outOfStock()) {
          <span class="badge absolute left-2 bottom-2 bg-white">Sold out</span>
        } @else if (p.stockQuantity <= 5) {
          <span class="badge absolute left-2 bottom-2 bg-saffron">Only {{ p.stockQuantity }} left</span>
        }
      </a>

      <button type="button" class="absolute right-5 top-5 grid size-9 place-items-center rounded-full border-2 border-ink bg-white transition-colors hover:bg-coral-soft"
        [class.text-coral]="liked()" (click)="wishlist.toggle(p.id)" [attr.aria-pressed]="liked()" [attr.aria-label]="liked() ? 'Remove from favourites' : 'Save to favourites'">
        <app-icon name="heart" size="1.05rem" [filled]="liked()" />
      </button>

      <div class="flex flex-1 flex-col gap-1 px-1 pt-3">
        <a [routerLink]="['/category', p.categoryId]" class="w-fit text-xs font-semibold text-ink-soft hover:text-cobalt">{{ p.categoryName }}</a>
        <a [routerLink]="['/product', p.id]" class="line-clamp-2 font-display text-[17px] font-semibold leading-snug hover:text-cobalt">{{ p.name }}</a>
        @if (p.reviewsCount > 0) {
          <div class="flex items-center gap-1.5 text-sm text-ink-soft">
            <app-stars [value]="p.averageRating" /> <span>({{ p.reviewsCount }})</span>
          </div>
        }
        <div class="mt-auto flex items-end justify-between gap-2 pt-2">
          <div class="text-lg"><app-price [price]="p.price" [final]="p.priceAfterDiscount" /></div>
          <button type="button" class="btn btn-primary btn-sm" [disabled]="outOfStock()" (click)="cart.add(p.id)" [attr.aria-label]="'Add ' + p.name + ' to basket'">
            <app-icon name="plus" size="1rem" /> Add
          </button>
        </div>
      </div>
    </article>`,
})
export class ProductCardComponent {
  api = inject(ApiService);
  cart = inject(CartService);
  wishlist = inject(WishlistService);
  product = input.required<Product>();
  mood = computed(() => moodFor(this.product().categoryId));
  outOfStock = computed(() => this.product().stockQuantity <= 0);
  liked = computed(() => this.wishlist.has(this.product().id));
}
