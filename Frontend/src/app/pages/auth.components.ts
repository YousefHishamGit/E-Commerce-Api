import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { ToastService } from '../core/toast.service';
import { LogoComponent } from '../layout/logo.component';
import { IconComponent } from '../shared/icon.component';

export const strongPassword = (c: AbstractControl): ValidationErrors | null => {
  const v = (c.value ?? '') as string;
  if (!v) return null;
  return v.length >= 8 && v.length <= 64 && /[a-z]/.test(v) && /[A-Z]/.test(v) && /\d/.test(v) ? null : { weak: true };
};
export const matches = (a: string, b: string) => (g: AbstractControl): ValidationErrors | null =>
  g.get(a)?.value === g.get(b)?.value ? null : { mismatch: true };

/** Two-column frame used by every auth screen. */
@Component({
  selector: 'app-auth-frame',
  standalone: true,
  imports: [LogoComponent, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="container-x grid min-h-[78vh] items-center gap-10 py-10 lg:grid-cols-2 lg:py-16">
      <div class="card mx-auto w-full max-w-md p-7 sm:p-9">
        <app-logo />
        <h1 class="mt-5 text-4xl font-extrabold">{{ title() }}</h1>
        <p class="mt-1.5 text-ink-soft">{{ subtitle() }}</p>
        <div class="mt-6"><ng-content /></div>
      </div>

      <div class="relative hidden aspect-[4/4.2] w-full max-w-lg justify-self-center rounded-[2.5rem] border-2 border-ink bg-saffron shadow-pop-lg lg:block" aria-hidden="true">
        <div class="absolute left-8 top-10 w-44 -rotate-6 rounded-2xl border-2 border-ink bg-white p-4 shadow-pop"><app-icon name="truck" size="2rem" /><p class="mt-2 font-display font-bold leading-tight">Delivered in 2–4 days</p></div>
        <div class="absolute right-8 top-1/3 w-48 rotate-3 rounded-2xl border-2 border-ink bg-cobalt p-4 text-white shadow-pop"><app-icon name="heart" size="2rem" /><p class="mt-2 font-display font-bold leading-tight">Save favourites for later</p></div>
        <div class="absolute bottom-10 left-14 w-52 -rotate-2 rounded-2xl border-2 border-ink bg-coral p-4 text-white shadow-pop"><app-icon name="shield" size="2rem" /><p class="mt-2 font-display font-bold leading-tight">Pay by card, wallet or cash</p></div>
      </div>
    </div>`,
})
export class AuthFrameComponent {
  title = input.required<string>();
  subtitle = input('');
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthFrameComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-auth-frame title="Welcome back" subtitle="Sign in to see your basket, favourites and orders.">
      <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4" novalidate>
        <div>
          <label class="label" for="email">Email</label>
          <input id="email" type="email" class="field" formControlName="email" autocomplete="email" placeholder="you@example.com" />
          @if (form.controls.email.touched && form.controls.email.invalid) { <p class="hint-error">Enter a valid email address.</p> }
        </div>
        <div>
          <div class="flex items-center justify-between"><label class="label" for="password">Password</label><a routerLink="/forgot-password" class="mb-1.5 text-sm font-semibold text-cobalt hover:underline">Forgot it?</a></div>
          <input id="password" type="password" class="field" formControlName="password" autocomplete="current-password" />
          @if (form.controls.password.touched && form.controls.password.invalid) { <p class="hint-error">Enter your password.</p> }
        </div>
        @if (error()) { <p class="rounded-xl border-2 border-coral bg-coral-soft px-3 py-2 text-sm font-medium" role="alert">{{ error() }}</p> }
        <button class="btn btn-primary btn-lg w-full" [disabled]="busy()">{{ busy() ? 'Signing in…' : 'Sign in' }}</button>
        <p class="text-center text-sm text-ink-soft">New to Mazaj? <a routerLink="/register" [queryParams]="route.snapshot.queryParams" class="font-semibold text-cobalt hover:underline">Create an account</a></p>
      </form>
    </app-auth-frame>`,
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);
  form = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]], password: ['', Validators.required] });
  busy = signal(false);
  error = signal('');

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true); this.error.set('');
    const { email, password } = this.form.getRawValue();
    this.auth.login(email.trim(), password).subscribe({
      next: r => {
        const back = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigateByUrl(back && back.startsWith('/') && !back.startsWith('//') ? back : r.role === 'Admin' ? '/admin' : '/');
      },
      error: e => { this.error.set(errorMessage(e, 'Wrong email or password.')); this.busy.set(false); },
    });
  }
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthFrameComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-auth-frame title="Join Mazaj" subtitle="Create a free account to start shopping your mood.">
      <form [formGroup]="form" (ngSubmit)="submit()" class="grid gap-4 sm:grid-cols-2" novalidate>
        <div>
          <label class="label" for="fn">First name</label>
          <input id="fn" class="field" formControlName="firstName" autocomplete="given-name" />
          @if (bad('firstName')) { <p class="hint-error">2 to 50 letters.</p> }
        </div>
        <div>
          <label class="label" for="ln">Last name</label>
          <input id="ln" class="field" formControlName="lastName" autocomplete="family-name" />
          @if (bad('lastName')) { <p class="hint-error">2 to 50 letters.</p> }
        </div>
        <div class="sm:col-span-2">
          <label class="label" for="em">Email</label>
          <input id="em" type="email" class="field" formControlName="email" autocomplete="email" />
          @if (bad('email')) { <p class="hint-error">Enter a valid email address.</p> }
        </div>
        <div>
          <label class="label" for="ph">Phone</label>
          <input id="ph" type="tel" class="field" formControlName="phone" autocomplete="tel" placeholder="01012345678" />
          @if (bad('phone')) { <p class="hint-error">10 to 15 digits, optional + at the start.</p> }
        </div>
        <div>
          <span class="label">Gender</span>
          <div class="flex gap-2">
            @for (g of ['Male', 'Female']; track g) {
              <label class="chip flex-1 cursor-pointer justify-center" [class.is-active]="form.controls.gender.value === g">
                <input type="radio" class="sr-only" formControlName="gender" [value]="g" /> {{ g }}
              </label>
            }
          </div>
        </div>
        <div>
          <label class="label" for="pw">Password</label>
          <input id="pw" type="password" class="field" formControlName="password" autocomplete="new-password" />
          @if (bad('password')) { <p class="hint-error">8+ characters with upper, lower case and a number.</p> }
        </div>
        <div>
          <label class="label" for="cpw">Confirm password</label>
          <input id="cpw" type="password" class="field" formControlName="confirmPassword" autocomplete="new-password" />
          @if (form.controls.confirmPassword.touched && form.hasError('mismatch')) { <p class="hint-error">Passwords do not match.</p> }
        </div>
        @if (error()) { <p class="rounded-xl border-2 border-coral bg-coral-soft px-3 py-2 text-sm font-medium sm:col-span-2" role="alert">{{ error() }}</p> }
        <button class="btn btn-primary btn-lg w-full sm:col-span-2" [disabled]="busy()">{{ busy() ? 'Creating account…' : 'Create account' }}</button>
        <p class="text-center text-sm text-ink-soft sm:col-span-2">Already a member? <a routerLink="/login" class="font-semibold text-cobalt hover:underline">Sign in</a></p>
      </form>
    </app-auth-frame>`,
})
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);
  form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.pattern(/^\+?[0-9]{10,15}$/)]],
    gender: ['Male' as 'Male' | 'Female', Validators.required],
    password: ['', [Validators.required, strongPassword]],
    confirmPassword: ['', Validators.required],
  }, { validators: matches('password', 'confirmPassword') });
  busy = signal(false);
  error = signal('');

  bad(k: 'firstName' | 'lastName' | 'email' | 'phone' | 'password') { const c = this.form.controls[k]; return c.touched && c.invalid; }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true); this.error.set('');
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => {
        this.toast.success('Welcome to Mazaj! Check your email to confirm your account.');
        const back = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigateByUrl(back && back.startsWith('/') && !back.startsWith('//') ? back : '/');
      },
      error: e => { this.error.set(errorMessage(e)); this.busy.set(false); },
    });
  }
}

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthFrameComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-auth-frame title="Forgot your password?" subtitle="Enter your email and we will send you a link to choose a new one.">
      @if (sent()) {
        <p class="rounded-xl border-2 border-ink bg-mint-soft p-4 font-medium" role="status">If an account exists for that email, a reset link is on its way. It can take a minute to arrive.</p>
        <a routerLink="/login" class="btn btn-light mt-5 w-full">Back to sign in</a>
      } @else {
        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4" novalidate>
          <div>
            <label class="label" for="email">Email</label>
            <input id="email" type="email" class="field" formControlName="email" autocomplete="email" />
            @if (form.controls.email.touched && form.controls.email.invalid) { <p class="hint-error">Enter a valid email address.</p> }
          </div>
          <button class="btn btn-primary btn-lg w-full" [disabled]="busy()">{{ busy() ? 'Sending…' : 'Send reset link' }}</button>
          <a routerLink="/login" class="block text-center text-sm font-semibold text-cobalt hover:underline">Back to sign in</a>
        </form>
      }
    </app-auth-frame>`,
})
export class ForgotPasswordComponent {
  private auth = inject(AuthService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);
  form = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]] });
  busy = signal(false);
  sent = signal(false);
  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true);
    this.auth.forgotPassword(this.form.getRawValue().email.trim()).subscribe({
      next: () => { this.sent.set(true); this.busy.set(false); },
      error: e => { this.toast.error(errorMessage(e)); this.busy.set(false); },
    });
  }
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthFrameComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-auth-frame title="Choose a new password" subtitle="Pick something you have not used before.">
      @if (!email || !token) {
        <p class="rounded-xl border-2 border-ink bg-coral-soft p-4 font-medium" role="alert">This reset link is incomplete. Request a new one.</p>
        <a routerLink="/forgot-password" class="btn btn-primary mt-5 w-full">Request a new link</a>
      } @else {
        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4" novalidate>
          <div>
            <label class="label" for="pw">New password</label>
            <input id="pw" type="password" class="field" formControlName="newPassword" autocomplete="new-password" />
            @if (form.controls.newPassword.touched && form.controls.newPassword.invalid) { <p class="hint-error">8+ characters with upper, lower case and a number.</p> }
          </div>
          <div>
            <label class="label" for="cpw">Confirm new password</label>
            <input id="cpw" type="password" class="field" formControlName="confirmNewPassword" autocomplete="new-password" />
            @if (form.controls.confirmNewPassword.touched && form.hasError('mismatch')) { <p class="hint-error">Passwords do not match.</p> }
          </div>
          @if (error()) { <p class="rounded-xl border-2 border-coral bg-coral-soft px-3 py-2 text-sm font-medium" role="alert">{{ error() }}</p> }
          <button class="btn btn-primary btn-lg w-full" [disabled]="busy()">{{ busy() ? 'Saving…' : 'Save new password' }}</button>
        </form>
      }
    </app-auth-frame>`,
})
export class ResetPasswordComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);
  private q = inject(ActivatedRoute).snapshot.queryParamMap;
  email = this.q.get('email') ?? '';
  token = this.q.get('token') ?? '';
  form = this.fb.nonNullable.group({ newPassword: ['', [Validators.required, strongPassword]], confirmNewPassword: ['', Validators.required] }, { validators: matches('newPassword', 'confirmNewPassword') });
  busy = signal(false);
  error = signal('');
  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true); this.error.set('');
    this.auth.resetPassword({ email: this.email, token: this.token, ...this.form.getRawValue() }).subscribe({
      next: () => { this.toast.success('Password updated. Sign in with your new password.'); this.router.navigate(['/login']); },
      error: e => { this.error.set(errorMessage(e, 'This link has expired. Request a new one.')); this.busy.set(false); },
    });
  }
}

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [RouterLink, AuthFrameComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-auth-frame title="Confirming your email" subtitle="One moment…">
      @switch (state()) {
        @case ('working') { <div class="mx-auto size-10 animate-spin rounded-full border-4 border-cobalt border-t-transparent"></div> }
        @case ('ok') {
          <p class="rounded-xl border-2 border-ink bg-mint-soft p-4 font-medium" role="status">Your email is confirmed. Thank you!</p>
          <a routerLink="/shop" class="btn btn-primary mt-5 w-full">Start shopping</a>
        }
        @default {
          <p class="rounded-xl border-2 border-ink bg-coral-soft p-4 font-medium" role="alert">This confirmation link is invalid or has expired.</p>
          <a routerLink="/account" class="btn btn-light mt-5 w-full">Go to my account</a>
        }
      }
    </app-auth-frame>`,
})
export class ConfirmEmailComponent {
  state = signal<'working' | 'ok' | 'fail'>('working');
  constructor() {
    const q = inject(ActivatedRoute).snapshot.queryParamMap;
    const email = q.get('email'), token = q.get('token');
    if (!email || !token) { this.state.set('fail'); return; }
    inject(AuthService).confirmEmail(email, token).subscribe({ next: () => this.state.set('ok'), error: () => this.state.set('fail') });
  }
}
