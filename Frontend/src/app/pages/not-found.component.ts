import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container-x grid min-h-[60vh] place-items-center py-16 text-center">
      <div>
        <p class="font-display text-[clamp(6rem,22vw,13rem)] font-extrabold leading-none text-cobalt [text-shadow:6px_6px_0_var(--color-ink)]">404</p>
        <h1 class="mt-4 text-3xl font-bold sm:text-4xl">This page is not in the mood.</h1>
        <p class="mt-2 text-ink-soft">The link may be old, or the page may have moved.</p>
        <div class="mt-6 flex flex-wrap justify-center gap-3"><a routerLink="/" class="btn btn-primary btn-lg">Go home</a><a routerLink="/shop" class="btn btn-light btn-lg">Browse the shop</a></div>
      </div>
    </section>`,
})
export class NotFoundComponent {}
