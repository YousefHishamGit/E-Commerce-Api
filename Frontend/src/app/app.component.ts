import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastService } from './core/toast.service';
import { IconComponent } from './shared/icon.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <router-outlet />
    <div class="pointer-events-none fixed inset-x-0 top-20 z-[60] flex flex-col items-center gap-2 px-4" aria-live="polite">
      @for (t of toast.toasts(); track t.id) {
        <div class="pointer-events-auto flex max-w-md animate-pop-in items-center gap-3 rounded-2xl border-2 border-ink px-4 py-3 font-medium shadow-pop"
             [class]="t.kind === 'error' ? 'bg-coral text-white' : t.kind === 'success' ? 'bg-mint text-ink' : 'bg-white text-ink'" [attr.role]="t.kind === 'error' ? 'alert' : 'status'">
          <app-icon [name]="t.kind === 'error' ? 'alert' : t.kind === 'success' ? 'check' : 'sparkle'" />
          <span class="flex-1">{{ t.text }}</span>
          <button (click)="toast.dismiss(t.id)" aria-label="Dismiss"><app-icon name="close" size="1rem" /></button>
        </div>
      }
    </div>
  `,
})
export class AppComponent {
  toast = inject(ToastService);
}
