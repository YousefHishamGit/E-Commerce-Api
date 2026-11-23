import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IconComponent } from './icon.component';
import { EgpPipe } from './egp.pipe';

/** Star rating (read-only, or interactive with `editable`). */
@Component({
  selector: 'app-stars',
  standalone: true,
  imports: [IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="inline-flex items-center gap-0.5" [attr.aria-label]="value() + ' out of 5 stars'" role="img">
      @for (s of [1,2,3,4,5]; track s) {
        @if (editable()) {
          <button type="button" class="p-0.5 text-saffron" [attr.aria-label]="s + ' stars'" (click)="changed.emit(s)">
            <app-icon name="star" [size]="size()" [filled]="s <= value()" />
          </button>
        } @else {
          <span class="text-saffron"><app-icon name="star" [size]="size()" [filled]="s <= rounded()" /></span>
        }
      }
    </span>`,
})
export class StarsComponent {
  value = input(0);
  size = input('1rem');
  editable = input(false);
  changed = output<number>();
  rounded = computed(() => Math.round(this.value()));
}

@Component({
  selector: 'app-price',
  standalone: true,
  imports: [EgpPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="font-display font-bold">{{ (final() ?? price()) | egp:'1.0-2' }}</span>
    @if (final() != null && final()! < price()) {
      <span class="ml-1.5 text-sm text-ink-soft line-through">{{ price() | egp:'1.0-2' }}</span>
    }`,
})
export class PriceComponent {
  price = input.required<number>();
  final = input<number | null | undefined>(null);
}

@Component({
  selector: 'app-empty',
  standalone: true,
  imports: [RouterLink, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="card-flat mx-auto flex max-w-lg flex-col items-center gap-3 border-dashed px-6 py-14 text-center">
      <span class="grid size-16 place-items-center rounded-2xl border-2 border-ink bg-saffron shadow-pop-sm"><app-icon [name]="icon()" size="1.75rem" /></span>
      <h3 class="text-2xl font-bold">{{ title() }}</h3>
      @if (text()) { <p class="max-w-sm text-ink-soft">{{ text() }}</p> }
      @if (cta() && link()) { <a [routerLink]="link()" class="btn btn-primary mt-2">{{ cta() }}</a> }
      <ng-content />
    </div>`,
})
export class EmptyComponent {
  icon = input('package');
  title = input.required<string>();
  text = input('');
  cta = input('');
  link = input('');
}

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (totalPages() > 1) {
      <nav class="flex items-center justify-center gap-2" aria-label="Pagination">
        <button class="btn btn-light btn-sm" [disabled]="page() <= 1" (click)="go.emit(page() - 1)" aria-label="Previous page"><app-icon name="chevronLeft" size="1rem" /></button>
        @for (p of pages(); track $index) {
          @if (p === 0) { <span class="px-1 text-ink-soft">…</span> }
          @else {
            <button class="btn btn-sm min-w-9" [class.btn-primary]="p === page()" [class.btn-light]="p !== page()" [attr.aria-current]="p === page() ? 'page' : null" (click)="go.emit(p)">{{ p }}</button>
          }
        }
        <button class="btn btn-light btn-sm" [disabled]="page() >= totalPages()" (click)="go.emit(page() + 1)" aria-label="Next page"><app-icon name="chevronRight" size="1rem" /></button>
      </nav>
    }`,
})
export class PaginationComponent {
  page = input.required<number>();
  totalPages = input.required<number>();
  go = output<number>();
  pages = computed(() => {
    const t = this.totalPages(), c = this.page();
    if (t <= 7) return Array.from({ length: t }, (_, i) => i + 1);
    const out: number[] = [1];
    if (c > 3) out.push(0);
    for (let p = Math.max(2, c - 1); p <= Math.min(t - 1, c + 1); p++) out.push(p);
    if (c < t - 2) out.push(0);
    out.push(t);
    return out;
  });
}

@Component({
  selector: 'app-spinner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="inline-block size-5 animate-spin rounded-full border-[3px] border-current border-t-transparent" role="status" aria-label="Loading"></span>`,
})
export class SpinnerComponent {}

/** Grid of placeholder tiles while products load. */
@Component({
  selector: 'app-product-skeleton',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @for (i of items(); track i) {
      <div class="card-flat overflow-hidden p-3">
        <div class="skeleton aspect-square rounded-xl"></div>
        <div class="skeleton mt-3 h-4 w-3/4 rounded"></div>
        <div class="skeleton mt-2 h-4 w-1/3 rounded"></div>
      </div>
    }`,
})
export class ProductSkeletonComponent {
  count = input(8);
  items = computed(() => Array.from({ length: this.count() }, (_, i) => i));
}
