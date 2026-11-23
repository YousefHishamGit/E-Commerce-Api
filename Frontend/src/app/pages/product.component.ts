import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { CartService } from '../core/cart.service';
import { Paginated, Product, Review } from '../core/models';
import { ToastService } from '../core/toast.service';
import { WishlistService } from '../core/wishlist.service';
import { moodFor } from '../core/utils';
import { IconComponent } from '../shared/icon.component';
import { ProductCardComponent } from '../shared/product-card.component';
import { EmptyComponent, PriceComponent, StarsComponent } from '../shared/ui.components';

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [RouterLink, FormsModule, DatePipe, IconComponent, PriceComponent, StarsComponent, ProductCardComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="container-x pt-6 lg:pt-10">
      @let p = product();
      @if (loading()) {
        <div class="grid gap-10 lg:grid-cols-2"><div class="skeleton aspect-square rounded-3xl"></div><div class="space-y-4"><div class="skeleton h-10 w-3/4 rounded"></div><div class="skeleton h-6 w-1/3 rounded"></div><div class="skeleton h-32 rounded"></div></div></div>
      } @else if (p) {
        <nav class="mb-5 flex items-center gap-1.5 text-sm text-ink-soft" aria-label="Breadcrumb">
          <a routerLink="/shop" class="hover:text-cobalt">Shop</a><app-icon name="chevronRight" size="0.9rem" />
          <a [routerLink]="['/category', p.categoryId]" class="hover:text-cobalt">{{ p.categoryName }}</a><app-icon name="chevronRight" size="0.9rem" />
          <span class="truncate font-medium text-ink">{{ p.name }}</span>
        </nav>

        <div class="grid gap-8 lg:grid-cols-2 lg:gap-14">
          <!-- Gallery -->
          <div>
            <div class="card overflow-hidden" [class]="mood().soft">
              <img [src]="api.img(activeImage())" [alt]="p.name" class="aspect-square w-full object-cover" />
            </div>
            @if (gallery().length > 1) {
              <ul class="mt-4 flex gap-3">
                @for (img of gallery(); track img) {
                  <li>
                    <button class="size-20 overflow-hidden rounded-xl border-2 transition-all" [class]="img === activeImage() ? 'border-ink shadow-pop-sm' : 'border-line opacity-80 hover:opacity-100'" (click)="activeImage.set(img)" [attr.aria-label]="'Show image'" [attr.aria-current]="img === activeImage()">
                      <img [src]="api.img(img)" alt="" class="size-full object-cover" />
                    </button>
                  </li>
                }
              </ul>
            }
          </div>

          <!-- Details -->
          <div>
            <a [routerLink]="['/category', p.categoryId]" class="badge" [class]="mood().bg + ' ' + mood().fg">{{ mood().emoji }} {{ p.categoryName }}</a>
            <h1 class="mt-3 text-4xl font-extrabold sm:text-5xl">{{ p.name }}</h1>

            <div class="mt-3 flex items-center gap-2">
              <app-stars [value]="p.averageRating" size="1.2rem" />
              <a href="#reviews" class="text-sm text-ink-soft underline-offset-2 hover:underline">{{ p.reviewsCount }} {{ p.reviewsCount === 1 ? 'review' : 'reviews' }}</a>
            </div>

            <div class="mt-5 flex flex-wrap items-center gap-3 text-3xl">
              <app-price [price]="p.price" [final]="p.priceAfterDiscount" />
              @if (p.hasOffer) { <span class="badge bg-coral text-base text-white">{{ p.offerTitle || 'On offer' }}</span> }
            </div>

            @if (p.description) { <p class="mt-5 max-w-prose whitespace-pre-line text-lg text-ink-soft">{{ p.description }}</p> }

            <div class="mt-6 flex items-center gap-2 text-sm font-semibold">
              @if (p.stockQuantity <= 0) { <span class="size-2.5 rounded-full bg-coral"></span> Out of stock }
              @else if (p.stockQuantity <= 5) { <span class="size-2.5 rounded-full bg-saffron"></span> Only {{ p.stockQuantity }} left }
              @else { <span class="size-2.5 rounded-full bg-mint"></span> In stock }
            </div>

            <div class="mt-5 flex flex-wrap items-center gap-3">
              <div class="inline-flex items-center rounded-full border-2 border-ink bg-white">
                <button class="grid size-11 place-items-center" (click)="qty.set(Math.max(1, qty() - 1))" aria-label="Decrease quantity"><app-icon name="minus" size="1rem" /></button>
                <span class="w-8 text-center font-bold" aria-live="polite">{{ qty() }}</span>
                <button class="grid size-11 place-items-center" (click)="qty.set(Math.min(p.stockQuantity, qty() + 1))" aria-label="Increase quantity"><app-icon name="plus" size="1rem" /></button>
              </div>
              <button class="btn btn-primary btn-lg flex-1 sm:flex-none" [disabled]="p.stockQuantity <= 0" (click)="cart.add(p.id, qty())"><app-icon name="cart" /> Add to basket</button>
              <button class="btn btn-light btn-lg !px-4" (click)="wishlist.toggle(p.id)" [attr.aria-pressed]="wishlist.has(p.id)" [attr.aria-label]="wishlist.has(p.id) ? 'Remove from favourites' : 'Save to favourites'" [class.text-coral]="wishlist.has(p.id)">
                <app-icon name="heart" [filled]="wishlist.has(p.id)" />
              </button>
            </div>

            <ul class="mt-8 grid gap-3 text-sm sm:grid-cols-3">
              <li class="card-flat flex items-center gap-2 p-3"><app-icon name="truck" /> Delivery in 2–4 days</li>
              <li class="card-flat flex items-center gap-2 p-3"><app-icon name="shield" /> Secure payment</li>
              <li class="card-flat flex items-center gap-2 p-3"><app-icon name="refresh" /> Cancel while pending</li>
            </ul>
          </div>
        </div>

        <!-- Reviews -->
        <section id="reviews" class="mt-20 scroll-mt-24">
          <h2 class="text-3xl font-bold sm:text-4xl">What people say</h2>
          <div class="mt-6 grid gap-8 lg:grid-cols-[1fr_380px]">
            <div class="space-y-4">
              @for (r of reviews(); track r.id) {
                <article class="card-flat p-5">
                  <div class="flex flex-wrap items-center justify-between gap-2">
                    <div class="flex items-center gap-3">
                      <span class="grid size-10 place-items-center rounded-full border-2 border-ink bg-lilac font-display font-bold">{{ r.userFullName.charAt(0) }}</span>
                      <div>
                        <p class="font-semibold">{{ r.userFullName }}
                          @if (r.isVerifiedPurchase) { <span class="badge ml-1 bg-mint-soft !py-0 text-[11px]">Verified purchase</span> }
                        </p>
                        <p class="text-xs text-ink-soft">{{ r.createdAt | date:'d MMM y' }}</p>
                      </div>
                    </div>
                    <app-stars [value]="r.rating" />
                  </div>
                  @if (r.comment) { <p class="mt-3">{{ r.comment }}</p> }
                  @if (auth.user()?.userId === r.userId) { <button class="mt-2 text-sm font-semibold text-coral hover:underline" (click)="deleteReview(r.id)">Delete my review</button> }
                </article>
              } @empty {
                <app-empty icon="star" title="No reviews yet" text="Bought this one? Be the first to tell others what you think." />
              }
            </div>

            <form class="card h-fit space-y-3 p-5" (submit)="$event.preventDefault(); submitReview()">
              <h3 class="text-xl font-bold">Write a review</h3>
              @if (auth.isLoggedIn()) {
                <div><span class="label">Your rating</span><app-stars [value]="rating()" size="1.6rem" [editable]="true" (changed)="rating.set($event)" /></div>
                <div>
                  <label class="label" for="comment">Your thoughts (optional)</label>
                  <textarea id="comment" class="field min-h-24" maxlength="1000" [(ngModel)]="comment" name="comment" placeholder="What did you like or dislike?"></textarea>
                </div>
                <button class="btn btn-primary w-full" [disabled]="rating() === 0 || sending()">{{ sending() ? 'Sending…' : 'Post review' }}</button>
              } @else {
                <p class="text-ink-soft">Sign in to share your experience with this product.</p>
                <a routerLink="/login" [queryParams]="{ returnUrl: '/product/' + p.id }" class="btn btn-primary w-full">Sign in to review</a>
              }
            </form>
          </div>
        </section>

        @if (related().length) {
          <section class="mt-20">
            <h2 class="mb-6 text-3xl font-bold sm:text-4xl">More in {{ p.categoryName }}</h2>
            <div class="grid grid-cols-2 gap-4 lg:grid-cols-4 lg:gap-6">
              @for (r of related(); track r.id) { <app-product-card [product]="r" /> }
            </div>
          </section>
        }
      } @else {
        <app-empty icon="search" title="We could not find that product" text="It may have been removed." cta="Back to the shop" link="/shop" />
      }
    </div>
  `,
})
export class ProductComponent {
  api = inject(ApiService);
  cart = inject(CartService);
  wishlist = inject(WishlistService);
  auth = inject(AuthService);
  private toast = inject(ToastService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  Math = Math;

  product = signal<Product | null>(null);
  loading = signal(true);
  reviews = signal<Review[]>([]);
  related = signal<Product[]>([]);
  activeImage = signal<string | null>(null);
  qty = signal(1);
  rating = signal(0);
  comment = '';
  sending = signal(false);

  mood = computed(() => moodFor(this.product()?.categoryId));
  gallery = computed(() => {
    const p = this.product();
    if (!p) return [];
    const urls = p.images?.length ? p.images.map(i => i.url) : p.photo ? [p.photo] : [];
    return urls.length ? urls : [''];
  });

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(m => this.load(+m.get('id')!));
  }

  private load(id: number) {
    this.loading.set(true); this.qty.set(1);
    this.api.get<Product>(`Products/${id}`).subscribe({
      next: p => {
        this.product.set(p); this.loading.set(false);
        this.activeImage.set(p.photo ?? p.images?.[0]?.url ?? null);
        this.loadReviews(id);
        this.api.get<Paginated<Product>>('Products', { categoryId: p.categoryId, pageSize: 5 }).subscribe({
          next: r => this.related.set(r.data.filter(x => x.id !== p.id).slice(0, 4)), error: () => {},
        });
      },
      error: () => { this.product.set(null); this.loading.set(false); },
    });
  }

  private loadReviews(id: number) {
    this.api.get<Paginated<Review>>(`Reviews/product/${id}`, { pageSize: 20 }).subscribe({ next: r => this.reviews.set(r.data), error: () => {} });
  }

  submitReview() {
    const p = this.product(); if (!p || !this.rating()) return;
    this.sending.set(true);
    this.api.post<Review>('Reviews', { productId: p.id, rating: this.rating(), comment: this.comment.trim() || null }).subscribe({
      next: () => { this.toast.success('Thanks for your review'); this.rating.set(0); this.comment = ''; this.sending.set(false); this.load(p.id); },
      error: e => { this.toast.error(errorMessage(e)); this.sending.set(false); },
    });
  }

  deleteReview(id: number) {
    const p = this.product(); if (!p) return;
    this.api.delete<string>(`Reviews/${id}`).subscribe({ next: () => { this.toast.success('Review deleted'); this.load(p.id); }, error: e => this.toast.error(errorMessage(e)) });
  }
}
