import { Injectable, signal } from '@angular/core';

export interface Toast { id: number; kind: 'success' | 'error' | 'info'; text: string; }

@Injectable({ providedIn: 'root' })
export class ToastService {
  private next = 1;
  readonly toasts = signal<Toast[]>([]);

  success(text: string) { this.push('success', text); }
  error(text: string) { this.push('error', text, 6000); }
  info(text: string) { this.push('info', text); }

  dismiss(id: number) { this.toasts.update(t => t.filter(x => x.id !== id)); }

  private push(kind: Toast['kind'], text: string, ms = 3500) {
    const id = this.next++;
    this.toasts.update(t => [...t.slice(-3), { id, kind, text }]);
    setTimeout(() => this.dismiss(id), ms);
  }
}
