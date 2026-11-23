import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest } from 'rxjs';
import { ApiService } from '../core/api.service';
import { Category, Paginated, Product } from '../core/models';
import { moodFor } from '../core/utils';
import { IconComponent } from '../shared/icon.component';
import { ProductCardComponent } from '../shared/product-card.component';
import { EmptyComponent, PaginationComponent, ProductSkeletonComponent } from '../shared/ui.components';

const PAGE_SIZE = 12;

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [FormsModule, IconComponent, ProductCardComponent, ProductSkeletonComponent, EmptyComponent, PaginationComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container-x pt-8 lg:pt-12">
      <header class="mb-8">
        <h1 class="page-title">{{ title() }}</h1>
        <p class="mt-2 text-ink-soft">
          @if (result(); as r) { {{ r.totalCount }} {{ r.totalCount === 1 ? 'product' : 'products' }} }
          @else { Loading products… }
          @if (search()) { for “{{ search() }}” }
        </p>
      </header>

      <div class="grid gap-8 lg:grid-cols-[260px_1fr]">
        <!-- Filters -->
        <aside>
          <button class="btn btn-light btn-sm mb-3 lg:hidden" (click)="filtersOpen.set(!filtersOpen())" [attr.aria-expanded]="filtersOpen()"><app-icon name="filter" size="1rem" /> Filters</button>
          <div class="space-y-6 lg:block" [class.hidden]="!filtersOpen()">
            <div>
              <h2 class="mb-2 text-lg font-bold">Mood</h2>
              <div class="flex flex-wrap gap-2 lg:flex-col lg:items-stretch">
                <button class="chip justify-start" [attr.aria-pressed]="!categoryId()" (click)="set({ categoryId: null })">✨ Everything</button>
                @for (c of categories(); track c.id) {
                  <button class="chip justify-start" [attr.aria-pressed]="categoryId() === c.id" (click)="set({ categoryId: c.id })">
                    <span aria-hidden="true">{{ mood(c.id).emoji }}</span> {{ c.name }} <span class="ml-auto pl-2 text-xs opacity-60">{{ c.productCount }}</span>
                  </button>
                }
              </div>
            </div>

            <form (submit)="$event.preventDefault(); applyPrice()">
              <h2 class="mb-2 text-lg font-bold">Price (EGP)</h2>
              <div class="flex items-center gap-2">
                <input class="field !px-3" type="number" min="0" placeholder="Min" [(ngModel)]="minPrice" name="min" aria-label="Minimum price" />
                <span>–</span>
                <input class="field !px-3" type="number" min="0" placeholder="Max" [(ngModel)]="maxPrice" name="max" aria-label="Maximum price" />
              </div>
              <button class="btn btn-light btn-sm mt-3 w-full" type="submit">Apply price</button>
            </form>

            @if (hasFilters()) { <button class="btn btn-flat btn-sm w-full underline" (click)="clear()">Clear all filters</button> }
          </div>
        </aside>

        <!-- Results -->
        <div>
          <div class="mb-5 flex items-center justify-end gap-2">
            <label for="sort" class="text-sm font-semibold">Sort by</label>
            <select id="sort" class="field !w-auto !py-1.5 pr-8" [ngModel]="sort()" (ngModelChange)="set({ sort: $event })">
              <option value="">Name (A–Z)</option>
              <option value="name_desc">Name (Z–A)</option>
              <option value="price_asc">Price: low to high</option>
              <option value="price_desc">Price: high to low</option>
              <option value="rating">Best rated</option>
            </select>
          </div>

          <div class="grid grid-cols-2 gap-4 md:grid-cols-3 lg:gap-6">
            @if (loading()) { <app-product-skeleton [count]="9" /> }
            @else { @for (p of result()?.data ?? []; track p.id) { <app-product-card [product]="p" /> } }
          </div>

          @if (!loading() && error()) {
            <app-empty icon="alert" title="We could not load products" text="Check your connection and try again." />
          } @else if (!loading() && (result()?.data?.length ?? 0) === 0) {
            <app-empty icon="search" title="No matches" text="Try a different word, or widen your price range." cta="See everything" link="/shop" />
          }

          <div class="mt-10">
            <app-pagination [page]="page()" [totalPages]="result()?.totalPages ?? 1" (go)="set({ page: $event })" />
          </div>
        </div>
      </div>
    </section>
  `,
})
export class ShopComponent {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  categories = signal<Category[]>([]);
  result = signal<Paginated<Product> | null>(null);
  loading = signal(true);
  error = signal(false);
  filtersOpen = signal(false);

  categoryId = signal<number | null>(null);
  search = signal('');
  sort = signal('');
  page = signal(1);
  minPrice: number | null = null;
  maxPrice: number | null = null;

  mood = (id: number) => moodFor(id);
  title = computed(() => {
    if (this.search()) return 'Search results';
    const c = this.categories().find(x => x.id === this.categoryId());
    return c ? c.name : 'The whole shop';
  });
  hasFilters = computed(() => !!(this.categoryId() || this.search() || this.minPrice || this.maxPrice || this.sort()));

  constructor() {
    this.api.get<Category[]>('Categories').subscribe({ next: c => this.categories.set(c), error: () => {} });
    combineLatest([this.route.paramMap, this.route.queryParamMap]).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(([p, q]) => {
      const cat = p.get('id') ?? q.get('categoryId');
      this.categoryId.set(cat ? +cat : null);
      this.search.set(q.get('search') ?? '');
      this.sort.set(q.get('sort') ?? '');
      this.page.set(+(q.get('page') ?? 1) || 1);
      this.minPrice = q.get('minPrice') ? +q.get('minPrice')! : null;
      this.maxPrice = q.get('maxPrice') ? +q.get('maxPrice')! : null;
      this.load();
    });
  }

  set(patch: { categoryId?: number | null; sort?: string; page?: number }) {
    const resetPage = !('page' in patch);
    this.router.navigate(['/shop'], {
      queryParams: {
        categoryId: ('categoryId' in patch ? patch.categoryId : this.categoryId()) || null,
        sort: ('sort' in patch ? patch.sort : this.sort()) || null,
        page: resetPage ? null : (patch.page! > 1 ? patch.page : null),
        search: this.search() || null,
        minPrice: this.minPrice || null,
        maxPrice: this.maxPrice || null,
      },
    });
    if (patch.page) window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  applyPrice() { this.set({}); }
  clear() { this.minPrice = this.maxPrice = null; this.router.navigate(['/shop']); }

  private load() {
    this.loading.set(true); this.error.set(false);
    this.api.get<Paginated<Product>>('Products', {
      pageIndex: this.page(), pageSize: PAGE_SIZE, categoryId: this.categoryId(), search: this.search(),
      sort: this.sort(), minPrice: this.minPrice, maxPrice: this.maxPrice,
    }).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => { this.result.set(null); this.error.set(true); this.loading.set(false); },
    });
  }
}
