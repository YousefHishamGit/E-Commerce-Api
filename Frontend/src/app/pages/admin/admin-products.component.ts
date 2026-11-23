import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../../core/api.service';
import { Category, Paginated, Product, ProductImage } from '../../core/models';
import { ToastService } from '../../core/toast.service';
import { IconComponent } from '../../shared/icon.component';
import { EmptyComponent, PaginationComponent } from '../../shared/ui.components';
import { EgpPipe } from '../../shared/egp.pipe';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [EgpPipe, RouterLink, FormsModule, IconComponent, PaginationComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="mb-6 flex flex-wrap items-center justify-between gap-3">
      <h1 class="text-4xl font-extrabold">Products</h1>
      <a routerLink="/admin/products/new" class="btn btn-primary"><app-icon name="plus" size="1.1rem" /> New product</a>
    </header>

    <form class="mb-4 flex gap-2" (submit)="$event.preventDefault(); page.set(1); load()">
      <input class="field max-w-sm" type="search" placeholder="Search products" [(ngModel)]="search" name="search" aria-label="Search products" />
      <button class="btn btn-light">Search</button>
    </form>

    <div class="card overflow-x-auto p-2">
      <table class="table-x min-w-[640px]">
        <thead><tr><th>Product</th><th>Category</th><th class="text-right">Price</th><th class="text-right">Stock</th><th class="w-28"></th></tr></thead>
        <tbody>
          @for (p of items(); track p.id) {
            <tr>
              <td><div class="flex items-center gap-3"><img [src]="api.img(p.photo)" alt="" class="size-12 rounded-lg border-2 border-ink object-cover" /><span class="font-semibold">{{ p.name }}</span></div></td>
              <td>{{ p.categoryName }}</td>
              <td class="text-right font-semibold">{{ p.price | egp:'1.0-2' }}</td>
              <td class="text-right"><span class="badge" [class]="p.stockQuantity === 0 ? 'bg-coral text-white' : p.stockQuantity <= 5 ? 'bg-saffron' : 'bg-mint-soft'">{{ p.stockQuantity }}</span></td>
              <td class="text-right"><a [routerLink]="['/admin/products', p.id]" class="btn btn-flat btn-sm" aria-label="Edit"><app-icon name="edit" size="1.05rem" /></a>
                <button class="btn btn-flat btn-sm text-coral" (click)="remove(p)" aria-label="Delete"><app-icon name="trash" size="1.05rem" /></button></td>
            </tr>
          }
        </tbody>
      </table>
      @if (!loading() && items().length === 0) { <div class="p-6"><app-empty title="No products found" text="Create your first product to start selling." cta="New product" link="/admin/products/new" /></div> }
    </div>
    <div class="mt-6"><app-pagination [page]="page()" [totalPages]="totalPages()" (go)="page.set($event); load()" /></div>
  `,
})
export class AdminProductsComponent {
  api = inject(ApiService);
  private toast = inject(ToastService);
  items = signal<Product[]>([]);
  loading = signal(true);
  page = signal(1);
  totalPages = signal(1);
  search = '';

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.api.get<Paginated<Product>>('Products', { pageIndex: this.page(), pageSize: 12, search: this.search }).subscribe({
      next: r => { this.items.set(r.data); this.totalPages.set(r.totalPages); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  remove(p: Product) {
    if (!confirm(`Delete “${p.name}”? This cannot be undone.`)) return;
    this.api.delete<string>(`Products/${p.id}`).subscribe({ next: () => { this.toast.success('Product deleted'); this.load(); }, error: e => this.toast.error(errorMessage(e)) });
  }
}

@Component({
  selector: 'app-admin-product-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a routerLink="/admin/products" class="mb-3 inline-flex items-center gap-1 text-sm font-semibold text-ink-soft hover:text-cobalt"><app-icon name="chevronLeft" size="1rem" /> Products</a>
    <h1 class="mb-6 text-4xl font-extrabold">{{ id() ? 'Edit product' : 'New product' }}</h1>

    <div class="grid items-start gap-6 xl:grid-cols-[1fr_380px]">
      <form class="card grid gap-4 p-6 sm:grid-cols-2" [formGroup]="form" (ngSubmit)="save()">
        <div class="sm:col-span-2"><label class="label" for="name">Name</label><input id="name" class="field" formControlName="name" />
          @if (form.controls.name.touched && form.controls.name.invalid) { <p class="hint-error">Name is required (2–150 characters).</p> }</div>
        <div><label class="label" for="price">Price (EGP)</label><input id="price" type="number" step="0.01" class="field" formControlName="price" />
          @if (form.controls.price.touched && form.controls.price.invalid) { <p class="hint-error">Price must be above 0.</p> }</div>
        <div><label class="label" for="stock">Stock</label><input id="stock" type="number" class="field" formControlName="stockQuantity" />
          @if (form.controls.stockQuantity.invalid) { <p class="hint-error">Stock cannot be negative.</p> }</div>
        <div class="sm:col-span-2"><label class="label" for="cat">Category</label>
          <select id="cat" class="field" formControlName="categoryId"><option [ngValue]="0" disabled>Choose a category</option>@for (c of categories(); track c.id) { <option [ngValue]="c.id">{{ c.name }}</option> }</select></div>
        <div class="sm:col-span-2"><label class="label" for="desc">Description</label><textarea id="desc" class="field min-h-32" maxlength="1000" formControlName="description"></textarea></div>
        <div class="flex gap-2 sm:col-span-2"><button class="btn btn-primary" [disabled]="form.invalid || busy()">{{ busy() ? 'Saving…' : id() ? 'Save changes' : 'Create product' }}</button><a routerLink="/admin/products" class="btn btn-light">Cancel</a></div>
      </form>

      <section class="card p-6">
        <h2 class="text-2xl font-bold">Images</h2>
        <p class="mb-4 text-sm text-ink-soft">Up to 4 images. The first one is the main photo.</p>
        @if (!id()) { <p class="rounded-xl border-2 border-dashed border-line p-4 text-sm text-ink-soft">Save the product first, then add images here.</p> }
        @else {
          <ul class="grid grid-cols-2 gap-3">
            @for (img of images(); track img.id) {
              <li class="relative"><img [src]="api.img(img.url)" alt="" class="aspect-square w-full rounded-xl border-2 border-ink object-cover" />
                @if (img.isMain) { <span class="badge absolute left-2 top-2 bg-saffron">Main</span> }
                <button class="absolute right-2 top-2 grid size-8 place-items-center rounded-full border-2 border-ink bg-white text-coral" (click)="removeImage(img)" aria-label="Delete image"><app-icon name="trash" size="0.95rem" /></button></li>
            }
          </ul>
          @if (images().length < 4) {
            <label class="btn btn-light mt-4 w-full cursor-pointer"><app-icon name="upload" size="1.1rem" /> {{ uploading() ? 'Uploading…' : 'Upload images' }}
              <input type="file" class="sr-only" accept="image/png,image/jpeg,image/webp" multiple (change)="upload($event)" [disabled]="uploading()" /></label>
          }
        }
      </section>
    </div>
  `,
})
export class AdminProductFormComponent {
  api = inject(ApiService);
  private toast = inject(ToastService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private destroyRef = inject(DestroyRef);

  id = signal<number | null>(null);
  categories = signal<Category[]>([]);
  images = signal<ProductImage[]>([]);
  busy = signal(false);
  uploading = signal(false);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    price: [0, [Validators.required, Validators.min(0.01), Validators.max(1000000)]],
    stockQuantity: [0, [Validators.required, Validators.min(0)]],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    description: [''],
  });

  constructor() {
    this.api.get<Category[]>('Categories').subscribe(c => this.categories.set(c));
    inject(ActivatedRoute).paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(m => {
      const raw = m.get('id');
      if (raw && raw !== 'new') { this.id.set(+raw); this.fetch(+raw); }
    });
  }

  private fetch(id: number) {
    this.api.get<Product>(`Products/${id}`).subscribe(p => {
      this.form.patchValue({ name: p.name, price: p.price, stockQuantity: p.stockQuantity, categoryId: p.categoryId, description: p.description ?? '' });
      this.images.set(p.images ?? []);
    });
  }

  save() {
    if (this.form.invalid) return;
    this.busy.set(true);
    const id = this.id();
    const body = this.form.getRawValue();
    (id ? this.api.put<Product>(`Products/${id}`, body) : this.api.post<Product>('Products', body)).subscribe({
      next: p => {
        this.busy.set(false);
        this.toast.success(id ? 'Product updated' : 'Product created. Now add some images.');
        if (!id) this.router.navigate(['/admin/products', p.id]);
      },
      error: e => { this.busy.set(false); this.toast.error(errorMessage(e)); },
    });
  }

  upload(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    if (!files.length || !this.id()) return;
    if (files.some(f => f.size > 5 * 1024 * 1024)) { this.toast.error('Each image must be under 5 MB.'); input.value = ''; return; }
    const form = new FormData();
    files.slice(0, 4 - this.images().length).forEach(f => form.append('files', f));
    this.uploading.set(true);
    this.api.upload<ProductImage[]>(`Products/${this.id()}/images`, form).subscribe({
      next: imgs => { this.images.set(imgs); this.uploading.set(false); input.value = ''; this.toast.success('Images uploaded'); },
      error: e => { this.uploading.set(false); input.value = ''; this.toast.error(errorMessage(e)); },
    });
  }

  removeImage(img: ProductImage) {
    this.api.delete<ProductImage[]>(`Products/${this.id()}/images/${img.id}`).subscribe({ next: imgs => this.images.set(imgs), error: e => this.toast.error(errorMessage(e)) });
  }
}
