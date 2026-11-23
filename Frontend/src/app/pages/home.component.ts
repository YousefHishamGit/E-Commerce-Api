import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { Category, Offer, Paginated, Product } from '../core/models';
import { moodFor } from '../core/utils';
import { IconComponent } from '../shared/icon.component';
import { ProductCardComponent } from '../shared/product-card.component';
import { EmptyComponent, ProductSkeletonComponent } from '../shared/ui.components';
import { EgpPipe } from '../shared/egp.pipe';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [EgpPipe, RouterLink, DatePipe, DecimalPipe, IconComponent, ProductCardComponent, ProductSkeletonComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- HERO: pick a mood, the whole block re-colours -->
    <section class="container-x grid items-center gap-8 pb-6 pt-8 lg:grid-cols-[1.1fr_1fr] lg:gap-12 lg:pt-14">
      <div>
        <h1 class="text-[clamp(2.75rem,7vw,5.5rem)] font-extrabold leading-[0.95]">
          What’s your mood today?
        </h1>
        <p class="mt-5 max-w-lg text-lg text-ink-soft">
          Pick a mood and we will show you things that match it. Fast delivery across Egypt, and you can pay by card, wallet or cash.
        </p>

        <div class="mt-7 flex flex-wrap gap-2.5" role="group" aria-label="Choose a mood">
          <button class="chip !px-5 !py-2.5 text-base" [attr.aria-pressed]="!selected()" (click)="select(null)">✨ Everything</button>
          @for (c of categories(); track c.id) {
            <button class="chip !px-5 !py-2.5 text-base" [attr.aria-pressed]="selected()?.id === c.id" (click)="select(c)">
              <span aria-hidden="true">{{ mood(c.id).emoji }}</span> {{ c.name }}
            </button>
          }
        </div>

        <div class="mt-8 flex flex-wrap items-center gap-3">
          <a routerLink="/shop" [queryParams]="selected() ? { categoryId: selected()!.id } : {}" class="btn btn-primary btn-lg">Shop {{ selected() ? selected()!.name.toLowerCase() : 'everything' }}</a>
          <a routerLink="/register" class="btn btn-light btn-lg">Create an account</a>
        </div>
      </div>

      <!-- Mood stage -->
      <div class="relative mx-auto aspect-[5/4.4] w-full max-w-xl rounded-[2.5rem] border-2 border-ink shadow-pop-lg transition-colors duration-500"
           [class]="stageClass()">
        @if (loadingProducts()) {
          <div class="absolute inset-0 grid place-items-center"><div class="size-10 animate-spin rounded-full border-4 border-current border-t-transparent"></div></div>
        } @else if (stage().length) {
          @for (p of stage(); track p.id; let i = $index) {
            <a [routerLink]="['/product', p.id]" class="absolute block w-[38%] animate-pop-in overflow-hidden rounded-2xl border-2 border-ink bg-white shadow-pop transition-transform duration-200 hover:z-20 hover:scale-105"
               [style.left]="positions[i].left" [style.top]="positions[i].top" [style.transform]="'rotate(' + positions[i].rot + 'deg)'" [style.animation-delay]="i * 90 + 'ms'" [style.z-index]="3 - i">
              <img [src]="api.img(p.photo)" [alt]="p.name" class="aspect-square w-full object-cover" />
              <div class="flex items-center justify-between gap-2 border-t-2 border-ink bg-white px-3 py-2 text-ink">
                <span class="truncate text-sm font-semibold">{{ p.name }}</span>
                <span class="shrink-0 font-display text-sm font-bold">{{ (p.priceAfterDiscount ?? p.price) | egp:'1.0-0' }}</span>
              </div>
            </a>
          }
        } @else {
          <div class="absolute inset-0 grid place-items-center p-8 text-center"><p class="font-display text-2xl font-bold">New things for this mood are on the way.</p></div>
        }
      </div>
    </section>

    <!-- Promise strip -->
    <section class="mt-10 overflow-hidden border-y-2 border-ink bg-saffron" aria-label="Our promises">
      <div class="marquee-track flex whitespace-nowrap py-3 font-display text-lg font-bold">
        @for (n of [0, 1]; track n) {
          <div class="flex shrink-0 items-center gap-10 pr-10" [attr.aria-hidden]="n === 1">
            @for (t of promises; track t) { <span class="inline-flex items-center gap-10">{{ t }} <app-icon name="sparkle" size="1.1rem" /></span> }
          </div>
        }
      </div>
    </section>

    <!-- Products for the chosen mood -->
    <section class="container-x mt-16">
      <div class="mb-6 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 class="text-3xl font-bold sm:text-4xl">{{ selected() ? 'For your ' + selected()!.name.toLowerCase() + ' mood' : 'Fresh in the shop' }}</h2>
          <p class="mt-1 text-ink-soft">{{ selected() ? (selected()!.description || 'Hand-picked for this mood.') : 'The newest things we have added.' }}</p>
        </div>
        <a routerLink="/shop" [queryParams]="selected() ? { categoryId: selected()!.id } : {}" class="btn btn-light btn-sm">See all <app-icon name="arrowRight" size="1rem" /></a>
      </div>
      <div class="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4 lg:gap-6">
        @if (loadingProducts()) { <app-product-skeleton [count]="8" /> }
        @else { @for (p of products(); track p.id) { <app-product-card [product]="p" /> } }
      </div>
      @if (!loadingProducts() && products().length === 0) {
        <app-empty title="Nothing in this mood yet" text="Try another mood or browse the whole shop." cta="Browse the shop" link="/shop" />
      }
    </section>

    <!-- Active offers -->
    @if (offers().length) {
      <section class="container-x mt-20">
        <h2 class="mb-6 text-3xl font-bold sm:text-4xl">Deals that end soon</h2>
        <div class="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
          @for (o of offers(); track o.id; let i = $index) {
            <a [routerLink]="o.productId ? ['/product', o.productId] : o.categoryId ? ['/category', o.categoryId] : ['/shop']"
               class="group card relative overflow-hidden p-6 transition-transform hover:-translate-y-1" [class]="mood(i).soft">
              <span class="font-display text-7xl font-extrabold leading-none">−{{ o.discountPercentage | number:'1.0-0' }}%</span>
              <h3 class="mt-3 text-2xl font-bold">{{ o.title }}</h3>
              <p class="mt-1 text-ink-soft">{{ o.productName || o.categoryName || 'Selected items' }}</p>
              <p class="mt-4 text-sm font-semibold">Ends {{ o.endDate | date:'d MMM' }}</p>
            </a>
          }
        </div>
      </section>
    }

    <!-- Why Mazaj -->
    <section class="container-x mt-24">
      <div class="grid gap-5 md:grid-cols-3">
        @for (v of values; track v.title) {
          <div class="card-flat p-6">
            <span class="grid size-12 place-items-center rounded-xl border-2 border-ink" [class]="v.bg"><app-icon [name]="v.icon" size="1.4rem" /></span>
            <h3 class="mt-4 text-xl font-bold">{{ v.title }}</h3>
            <p class="mt-1 text-ink-soft">{{ v.text }}</p>
          </div>
        }
      </div>
    </section>

    <!-- CTA -->
    <section class="container-x mt-24">
      <div class="card relative overflow-hidden bg-cobalt p-8 text-white sm:p-12">
        <div class="max-w-xl">
          <h2 class="text-4xl font-extrabold sm:text-5xl">Save what you love. Buy it when the mood hits.</h2>
          <p class="mt-3 text-lg text-white/80">A free account keeps your basket, favourites and orders in one place.</p>
          <a routerLink="/register" class="btn btn-accent btn-lg mt-6">Create your account</a>
        </div>
        <div class="pointer-events-none absolute -right-10 -top-10 size-56 rounded-full border-2 border-ink bg-saffron sm:size-72"></div>
        <div class="pointer-events-none absolute -bottom-16 right-24 size-40 rounded-full border-2 border-ink bg-coral"></div>
      </div>
    </section>
  `,
  styles: [`:host { display: block; } .chip[aria-pressed="true"] { box-shadow: var(--shadow-pop-sm); }`],
})
export class HomeComponent {
  api = inject(ApiService);
  categories = signal<Category[]>([]);
  selected = signal<Category | null>(null);
  products = signal<Product[]>([]);
  offers = signal<Offer[]>([]);
  loadingProducts = signal(true);

  stage = computed(() => this.products().filter(p => !!p.photo).slice(0, 3));
  stageClass = computed(() => { const m = moodFor(this.selected()?.id); return `${m.bg} ${m.fg}`; });

  readonly positions = [
    { left: '6%', top: '6%', rot: -6 },
    { left: '54%', top: '10%', rot: 5 },
    { left: '30%', top: '46%', rot: -2 },
  ];
  readonly promises = ['Free delivery over EGP 1,000', 'Pay by card, wallet or cash', 'Cancel while pending', 'Packed in Cairo', 'Secure checkout with Paymob'];
  readonly values = [
    { icon: 'truck', bg: 'bg-mint', title: 'Delivered in 2–4 days', text: 'We pack in Cairo and ship across Egypt. Delivery is free when your order passes EGP 1,000.' },
    { icon: 'shield', bg: 'bg-cobalt-soft', title: 'Pay the way you like', text: 'Card or mobile wallet through Paymob, or cash when your order arrives.' },
    { icon: 'refresh', bg: 'bg-saffron', title: 'Change your mind freely', text: 'Cancel any order while it is still pending, straight from your orders page.' },
  ];

  mood = (id: number) => moodFor(id);

  constructor() {
    this.api.get<Category[]>('Categories').subscribe({ next: c => this.categories.set(c.filter(x => x.productCount > 0 || c.length < 6).slice(0, 6)), error: () => {} });
    this.api.get<Offer[]>('Offers/active').subscribe({ next: o => this.offers.set(o.slice(0, 3)), error: () => {} });
    this.load();
  }

  select(c: Category | null) { this.selected.set(c); this.load(); }

  private load() {
    this.loadingProducts.set(true);
    this.api.get<Paginated<Product>>('Products', { pageIndex: 1, pageSize: 8, categoryId: this.selected()?.id }).subscribe({
      next: r => { this.products.set(r.data); this.loadingProducts.set(false); },
      error: () => { this.products.set([]); this.loadingProducts.set(false); },
    });
  }
}
