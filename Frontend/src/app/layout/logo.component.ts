import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-logo',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a routerLink="/" class="group inline-flex items-center gap-2" aria-label="Mazaj home">
      <svg viewBox="0 0 64 64" class="size-9 transition-transform duration-200 group-hover:-rotate-6" aria-hidden="true">
        <rect x="4" y="4" width="56" height="56" rx="18" fill="#2d3df0" stroke="#16142e" stroke-width="4"/>
        <path d="M16 46V20l16 18 16-18v26" fill="none" stroke="#ffc42e" stroke-width="7" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
      <span class="font-display text-[26px] font-extrabold tracking-tight" [class.text-white]="light()">mazaj</span>
    </a>`,
})
export class LogoComponent {
  light = input(false);
}
