import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { ApiService, errorMessage } from '../../core/api.service';
import { Category, Coupon, Offer, Order, OrderStatus, Paginated, Product } from '../../core/models';
import { ToastService } from '../../core/toast.service';
import { STATUS_STYLE, toLocalInput } from '../../core/utils';
import { IconComponent } from '../../shared/icon.component';
import { EmptyComponent, PaginationComponent } from '../../shared/ui.components';
import { EgpPipe } from '../../shared/egp.pipe';

/* ───────── Categories ───────── */
@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [ReactiveFormsModule, IconComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1 class="mb-6 text-4xl font-extrabold">Categories</h1>
    <div class="grid items-start gap-6 lg:grid-cols-[360px_1fr]">
      <form class="card space-y-4 p-6" [formGroup]="form" (ngSubmit)="save()">
        <h2 class="text-2xl font-bold">{{ editing() ? 'Edit category' : 'New category' }}</h2>
        <div><label class="label" for="n">Name</label><input id="n" class="field" formControlName="name" />
          @if (form.controls.name.touched && form.controls.name.invalid) { <p class="hint-error">2 to 100 characters.</p> }</div>
        <div><label class="label" for="d">Description</label><textarea id="d" class="field" rows="3" maxlength="500" formControlName="description"></textarea></div>
        <div class="flex gap-2"><button class="btn btn-primary" [disabled]="form.invalid">{{ editing() ? 'Save' : 'Create' }}</button>
          @if (editing()) { <button type="button" class="btn btn-light" (click)="reset()">Cancel</button> }</div>
      </form>
      <div class="space-y-3">
        @for (c of items(); track c.id) {
          <article class="card-flat flex items-center justify-between gap-3 p-4">
            <div><p class="font-display text-lg font-bold">{{ c.name }}</p><p class="text-sm text-ink-soft">{{ c.productCount }} products @if (c.description) { · {{ c.description }} }</p></div>
            <div class="flex"><button class="btn btn-flat btn-sm" (click)="edit(c)" aria-label="Edit"><app-icon name="edit" size="1.05rem" /></button>
              <button class="btn btn-flat btn-sm text-coral" (click)="remove(c)" aria-label="Delete"><app-icon name="trash" size="1.05rem" /></button></div>
          </article>
        } @empty { <app-empty icon="layers" title="No categories" text="Create one to start organising products." /> }
      </div>
    </div>
  `,
})
export class AdminCategoriesComponent {
  private api = inject(ApiService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);
  items = signal<Category[]>([]);
  editing = signal<number | null>(null);
  form = this.fb.nonNullable.group({ name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]], description: [''] });

  constructor() { this.load(); }
  load() { this.api.get<Category[]>('Categories').subscribe(c => this.items.set(c)); }
  edit(c: Category) { this.editing.set(c.id); this.form.patchValue({ name: c.name, description: c.description ?? '' }); }
  reset() { this.editing.set(null); this.form.reset(); }
  save() {
    const id = this.editing(); const v = this.form.getRawValue();
    (id ? this.api.put<Category>(`Categories/${id}`, v) : this.api.post<Category>('Categories', v)).subscribe({
      next: () => { this.toast.success(id ? 'Category updated' : 'Category created'); this.reset(); this.load(); }, error: e => this.toast.error(errorMessage(e)),
    });
  }
  remove(c: Category) {
    if (!confirm(`Delete “${c.name}”?`)) return;
    this.api.delete<string>(`Categories/${c.id}`).subscribe({ next: () => { this.toast.success('Category deleted'); this.load(); }, error: e => this.toast.error(errorMessage(e)) });
  }
}

/* ───────── Orders ───────── */
@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [EgpPipe, DatePipe, FormsModule, PaginationComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1 class="mb-6 text-4xl font-extrabold">Orders</h1>
    <div class="card overflow-x-auto p-2">
      <table class="table-x min-w-[760px]">
        <thead><tr><th>Order</th><th>Customer</th><th>Deliver to</th><th>Payment</th><th class="text-right">Total</th><th>Status</th></tr></thead>
        <tbody>
          @for (o of items(); track o.id) {
            <tr>
              <td><p class="font-semibold">{{ o.id.slice(0, 8) }}</p><p class="text-xs text-ink-soft">{{ o.createdAt | date:'d MMM y, h:mm a' }}</p></td>
              <td>{{ o.customerName }}</td>
              <td class="max-w-48 truncate">{{ o.shippingCity }}, {{ o.shippingStreet }}</td>
              <td>@if (o.payments[0]; as p) { <span class="badge" [class]="style(p.status)">{{ p.method === 'CashOnDelivery' ? 'COD' : 'Online' }} · {{ p.status }}</span> } @else { — }</td>
              <td class="text-right font-semibold">{{ o.totalPrice | egp:'1.0-0' }}</td>
              <td>
                <select class="field !w-auto !rounded-full !py-1 !text-sm font-semibold" [class]="style(o.status)" [ngModel]="o.status" (ngModelChange)="setStatus(o, $event)" [attr.aria-label]="'Status of order ' + o.id.slice(0, 8)">
                  @for (s of statuses; track s) { <option [value]="s">{{ s }}</option> }
                </select>
              </td>
            </tr>
          }
        </tbody>
      </table>
      @if (!loading() && items().length === 0) { <div class="p-6"><app-empty icon="truck" title="No orders yet" text="Orders will appear here as customers check out." /></div> }
    </div>
    <div class="mt-6"><app-pagination [page]="page()" [totalPages]="totalPages()" (go)="page.set($event); load()" /></div>
  `,
})
export class AdminOrdersComponent {
  private api = inject(ApiService);
  private toast = inject(ToastService);
  items = signal<Order[]>([]);
  loading = signal(true);
  page = signal(1);
  totalPages = signal(1);
  statuses: OrderStatus[] = ['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'];
  style = (s: string) => STATUS_STYLE[s] ?? '';
  constructor() { this.load(); }
  load() {
    this.loading.set(true);
    this.api.get<Paginated<Order>>('Orders/all', { pageIndex: this.page(), pageSize: 15 }).subscribe({
      next: r => { this.items.set(r.data); this.totalPages.set(r.totalPages); this.loading.set(false); }, error: () => this.loading.set(false),
    });
  }
  setStatus(o: Order, status: OrderStatus) {
    this.api.patch<Order>(`Orders/${o.id}/status`, { status }).subscribe({
      next: u => { this.items.update(l => l.map(x => (x.id === u.id ? u : x))); this.toast.success(`Order marked ${status.toLowerCase()}`); },
      error: e => { this.toast.error(errorMessage(e)); this.load(); },
    });
  }
}

/* ───────── Coupons ───────── */
@Component({
  selector: 'app-admin-coupons',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, IconComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="mb-6 flex items-center justify-between"><h1 class="text-4xl font-extrabold">Coupons</h1>
      <button class="btn btn-primary" (click)="open()"><app-icon name="plus" size="1.1rem" /> New coupon</button></header>

    @if (formOpen()) {
      <form class="card mb-6 grid gap-4 p-6 sm:grid-cols-2 lg:grid-cols-3" [formGroup]="form" (ngSubmit)="save()">
        <h2 class="text-2xl font-bold sm:col-span-2 lg:col-span-3">{{ editing() ? 'Edit coupon' : 'New coupon' }}</h2>
        <div><label class="label" for="code">Code</label><input id="code" class="field uppercase" formControlName="code" maxlength="50" /></div>
        <div><label class="label" for="type">Type</label><select id="type" class="field" formControlName="type"><option value="Percentage">Percentage (%)</option><option value="FixedAmount">Fixed amount (EGP)</option></select></div>
        <div><label class="label" for="val">Value</label><input id="val" type="number" step="0.01" class="field" formControlName="value" /></div>
        <div><label class="label" for="min">Minimum order (EGP)</label><input id="min" type="number" class="field" formControlName="minOrderAmount" /></div>
        <div><label class="label" for="max">Max uses</label><input id="max" type="number" class="field" formControlName="maxUsage" placeholder="Unlimited" /></div>
        <div></div>
        <div><label class="label" for="s">Starts</label><input id="s" type="datetime-local" class="field" formControlName="startDate" /></div>
        <div><label class="label" for="e">Ends</label><input id="e" type="datetime-local" class="field" formControlName="endDate" /></div>
        <div class="flex items-end gap-2"><button class="btn btn-primary" [disabled]="form.invalid">Save coupon</button><button type="button" class="btn btn-light" (click)="formOpen.set(false)">Cancel</button></div>
      </form>
    }

    <div class="card overflow-x-auto p-2">
      <table class="table-x min-w-[720px]">
        <thead><tr><th>Code</th><th>Discount</th><th>Valid</th><th>Used</th><th>Status</th><th class="w-28"></th></tr></thead>
        <tbody>
          @for (c of items(); track c.id) {
            <tr>
              <td class="font-mono font-bold">{{ c.code }}</td>
              <td>{{ c.type === 'Percentage' ? c.value + '%' : 'EGP ' + c.value }}@if (c.minOrderAmount) { <span class="block text-xs text-ink-soft">over EGP {{ c.minOrderAmount }}</span> }</td>
              <td class="text-sm">{{ c.startDate | date:'d MMM' }} – {{ c.endDate | date:'d MMM y' }}</td>
              <td>{{ c.usedCount }}{{ c.maxUsage ? ' / ' + c.maxUsage : '' }}</td>
              <td><button class="badge cursor-pointer" [class]="c.isActive ? 'bg-mint' : 'bg-white'" (click)="toggle(c)" [attr.aria-pressed]="c.isActive">{{ c.isActive ? 'Active' : 'Off' }}</button></td>
              <td class="text-right"><button class="btn btn-flat btn-sm" (click)="open(c)" aria-label="Edit"><app-icon name="edit" size="1.05rem" /></button><button class="btn btn-flat btn-sm text-coral" (click)="remove(c)" aria-label="Delete"><app-icon name="trash" size="1.05rem" /></button></td>
            </tr>
          }
        </tbody>
      </table>
      @if (items().length === 0) { <div class="p-6"><app-empty icon="ticket" title="No coupons yet" text="Create a code to reward your customers." /></div> }
    </div>
  `,
})
export class AdminCouponsComponent {
  private api = inject(ApiService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);
  items = signal<Coupon[]>([]);
  formOpen = signal(false);
  editing = signal<number | null>(null);
  form = this.fb.group({
    code: ['', [Validators.required, Validators.maxLength(50)]], type: ['Percentage', Validators.required],
    value: [10 as number | null, [Validators.required, Validators.min(0.01)]], minOrderAmount: [null as number | null], maxUsage: [null as number | null],
    startDate: [toLocalInput(), Validators.required], endDate: [toLocalInput(new Date(Date.now() + 30 * 864e5).toISOString()), Validators.required],
  });
  constructor() { this.load(); }
  load() { this.api.get<Coupon[]>('Coupons').subscribe(c => this.items.set(c)); }
  open(c?: Coupon) {
    this.editing.set(c?.id ?? null); this.formOpen.set(true);
    if (c) this.form.patchValue({ ...c, startDate: toLocalInput(c.startDate), endDate: toLocalInput(c.endDate) }); else this.form.reset({ type: 'Percentage', value: 10, startDate: toLocalInput(), endDate: toLocalInput(new Date(Date.now() + 30 * 864e5).toISOString()) });
  }
  save() {
    const v = this.form.getRawValue(); const id = this.editing();
    const body = { ...v, code: v.code!.trim().toUpperCase(), startDate: new Date(v.startDate!).toISOString(), endDate: new Date(v.endDate!).toISOString(), minOrderAmount: v.minOrderAmount || null, maxUsage: v.maxUsage || null };
    (id ? this.api.put<Coupon>(`Coupons/${id}`, body) : this.api.post<Coupon>('Coupons', body)).subscribe({
      next: () => { this.toast.success(id ? 'Coupon updated' : 'Coupon created'); this.formOpen.set(false); this.load(); }, error: e => this.toast.error(errorMessage(e)),
    });
  }
  toggle(c: Coupon) { this.api.patch<Coupon>(`Coupons/${c.id}/toggle`).subscribe({ next: u => this.items.update(l => l.map(x => (x.id === u.id ? u : x))), error: e => this.toast.error(errorMessage(e)) }); }
  remove(c: Coupon) {
    if (!confirm(`Delete coupon ${c.code}?`)) return;
    this.api.delete<string>(`Coupons/${c.id}`).subscribe({ next: () => { this.toast.success('Coupon deleted'); this.load(); }, error: e => this.toast.error(errorMessage(e)) });
  }
}

/* ───────── Offers ───────── */
@Component({
  selector: 'app-admin-offers',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, IconComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="mb-6 flex items-center justify-between"><h1 class="text-4xl font-extrabold">Offers</h1>
      <button class="btn btn-primary" (click)="open()"><app-icon name="plus" size="1.1rem" /> New offer</button></header>

    @if (formOpen()) {
      <form class="card mb-6 grid gap-4 p-6 sm:grid-cols-2 lg:grid-cols-3" [formGroup]="form" (ngSubmit)="save()">
        <h2 class="text-2xl font-bold sm:col-span-2 lg:col-span-3">{{ editing() ? 'Edit offer' : 'New offer' }}</h2>
        <div class="sm:col-span-2"><label class="label" for="t">Title</label><input id="t" class="field" formControlName="title" maxlength="150" /></div>
        <div><label class="label" for="p">Discount (%)</label><input id="p" type="number" min="1" max="100" class="field" formControlName="discountPercentage" /></div>
        <div class="sm:col-span-2 lg:col-span-3"><label class="label" for="ds">Description</label><input id="ds" class="field" formControlName="description" maxlength="500" /></div>
        <div><label class="label" for="pr">Applies to product</label><select id="pr" class="field" formControlName="productId"><option [ngValue]="null">None</option>@for (p of products(); track p.id) { <option [ngValue]="p.id">{{ p.name }}</option> }</select></div>
        <div><label class="label" for="ca">…or whole category</label><select id="ca" class="field" formControlName="categoryId"><option [ngValue]="null">None</option>@for (c of categories(); track c.id) { <option [ngValue]="c.id">{{ c.name }}</option> }</select></div>
        <div></div>
        <div><label class="label" for="s">Starts</label><input id="s" type="datetime-local" class="field" formControlName="startDate" /></div>
        <div><label class="label" for="e">Ends</label><input id="e" type="datetime-local" class="field" formControlName="endDate" /></div>
        <div class="flex items-end gap-2"><button class="btn btn-primary" [disabled]="form.invalid">Save offer</button><button type="button" class="btn btn-light" (click)="formOpen.set(false)">Cancel</button></div>
      </form>
    }

    <div class="card overflow-x-auto p-2">
      <table class="table-x min-w-[720px]">
        <thead><tr><th>Offer</th><th>Applies to</th><th>Discount</th><th>Valid</th><th>Status</th><th class="w-28"></th></tr></thead>
        <tbody>
          @for (o of items(); track o.id) {
            <tr>
              <td class="font-semibold">{{ o.title }}</td>
              <td>{{ o.productName || o.categoryName || 'Everything' }}</td>
              <td><span class="badge bg-coral text-white">−{{ o.discountPercentage }}%</span></td>
              <td class="text-sm">{{ o.startDate | date:'d MMM' }} – {{ o.endDate | date:'d MMM y' }}</td>
              <td><button class="badge cursor-pointer" [class]="o.isActive ? 'bg-mint' : 'bg-white'" (click)="toggle(o)" [attr.aria-pressed]="o.isActive">{{ o.isActive ? 'Active' : 'Off' }}</button></td>
              <td class="text-right"><button class="btn btn-flat btn-sm" (click)="open(o)" aria-label="Edit"><app-icon name="edit" size="1.05rem" /></button><button class="btn btn-flat btn-sm text-coral" (click)="remove(o)" aria-label="Delete"><app-icon name="trash" size="1.05rem" /></button></td>
            </tr>
          }
        </tbody>
      </table>
      @if (items().length === 0) { <div class="p-6"><app-empty icon="percent" title="No offers yet" text="Offers lower prices automatically for a product or category." /></div> }
    </div>
  `,
})
export class AdminOffersComponent {
  private api = inject(ApiService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);
  items = signal<Offer[]>([]);
  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  formOpen = signal(false);
  editing = signal<number | null>(null);
  form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(150)]], description: [''], discountPercentage: [10, [Validators.required, Validators.min(1), Validators.max(100)]],
    productId: [null as number | null], categoryId: [null as number | null],
    startDate: [toLocalInput(), Validators.required], endDate: [toLocalInput(new Date(Date.now() + 14 * 864e5).toISOString()), Validators.required],
  });
  constructor() {
    this.load();
    this.api.get<Paginated<Product>>('Products', { pageSize: 50 }).subscribe(r => this.products.set(r.data));
    this.api.get<Category[]>('Categories').subscribe(c => this.categories.set(c));
  }
  load() { this.api.get<Offer[]>('Offers').subscribe(o => this.items.set(o)); }
  open(o?: Offer) {
    this.editing.set(o?.id ?? null); this.formOpen.set(true);
    if (o) this.form.patchValue({ ...o, startDate: toLocalInput(o.startDate), endDate: toLocalInput(o.endDate) }); else this.form.reset({ discountPercentage: 10, startDate: toLocalInput(), endDate: toLocalInput(new Date(Date.now() + 14 * 864e5).toISOString()) });
  }
  save() {
    const v = this.form.getRawValue(); const id = this.editing();
    const body = { ...v, startDate: new Date(v.startDate!).toISOString(), endDate: new Date(v.endDate!).toISOString() };
    (id ? this.api.put<Offer>(`Offers/${id}`, body) : this.api.post<Offer>('Offers', body)).subscribe({
      next: () => { this.toast.success(id ? 'Offer updated' : 'Offer created'); this.formOpen.set(false); this.load(); }, error: e => this.toast.error(errorMessage(e)),
    });
  }
  toggle(o: Offer) { this.api.patch<Offer>(`Offers/${o.id}/toggle`).subscribe({ next: u => this.items.update(l => l.map(x => (x.id === u.id ? u : x))), error: e => this.toast.error(errorMessage(e)) }); }
  remove(o: Offer) {
    if (!confirm(`Delete offer “${o.title}”?`)) return;
    this.api.delete<string>(`Offers/${o.id}`).subscribe({ next: () => { this.toast.success('Offer deleted'); this.load(); }, error: e => this.toast.error(errorMessage(e)) });
  }
}
