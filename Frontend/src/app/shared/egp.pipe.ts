import { Pipe, PipeTransform } from '@angular/core';

/** Formats a number as Egyptian pounds: `EGP 1,850` (digits like Angular's '1.0-2'). */
@Pipe({ name: 'egp', standalone: true })
export class EgpPipe implements PipeTransform {
  private cache = new Map<string, Intl.NumberFormat>();

  transform(value: number | null | undefined, digits = '1.0-2'): string {
    if (value == null || isNaN(value)) return '—';
    const m = /^\d+\.(\d+)-(\d+)$/.exec(digits);
    const min = m ? +m[1] : 0, max = m ? +m[2] : 2;
    const key = `${min}-${max}`;
    let f = this.cache.get(key);
    if (!f) { f = new Intl.NumberFormat('en-US', { minimumFractionDigits: min, maximumFractionDigits: max }); this.cache.set(key, f); }
    return `EGP ${f.format(value)}`;
  }
}
