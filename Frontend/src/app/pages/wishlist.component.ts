import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { CartService } from '../core/cart.service';
import { WishlistService } from '../core/wishlist.service';
import { IconComponent } from '../shared/icon.component';
import { EmptyComponent } from '../shared/ui.components';
import { EgpPipe } from '../shared/egp.pipe';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [EgpPipe, RouterLink, IconComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container-x pt-8 lg:pt-12">
      <h1 class="page-title">Favourites</h1>
      @if (wishlist.items().length === 0) {
        <div class="mt-8"><app-empty icon="heart" title="No favourites yet" text="Tap the heart on anything you like and it will wait for you here." cta="Find something you like" link="/shop" /></div>
      } @else {
        <ul class="mt-8 grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4 lg:gap-6">
          @for (i of wishlist.items(); track i.id) {
            <li class="card-flat flex flex-col overflow-hidden p-3">
              <a [routerLink]="['/product', i.productId]" class="block overflow-hidden rounded-xl"><img [src]="api.img(i.productPhoto)" [alt]="i.productName" class="aspect-square w-full object-cover" /></a>
              <a [routerLink]="['/product', i.productId]" class="mt-3 line-clamp-2 font-display text-[17px] font-semibold hover:text-cobalt">{{ i.productName }}</a>
              <p class="font-display text-lg font-bold">{{ i.productPrice | egp:'1.0-2' }}</p>
              <div class="mt-auto flex gap-2 pt-3">
                <button class="btn btn-primary btn-sm flex-1" [disabled]="i.stockQuantity <= 0" (click)="cart.add(i.productId)">{{ i.stockQuantity <= 0 ? 'Sold out' : 'Add' }}</button>
                <button class="btn btn-light btn-sm !px-3" (click)="wishlist.toggle(i.productId)" aria-label="Remove from favourites"><app-icon name="trash" size="1rem" /></button>
              </div>
            </li>
          }
        </ul>
      }
    </section>
  `,
})
export class WishlistComponent {
  api = inject(ApiService);
  cart = inject(CartService);
  wishlist = inject(WishlistService);
}
