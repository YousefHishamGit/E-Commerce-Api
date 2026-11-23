import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService, errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { Address, User } from '../core/models';
import { ToastService } from '../core/toast.service';
import { IconComponent } from '../shared/icon.component';
import { EmptyComponent } from '../shared/ui.components';
import { matches, strongPassword } from './auth.components';

type Tab = 'profile' | 'addresses' | 'security';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [ReactiveFormsModule, IconComponent, EmptyComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="container-x pt-8 lg:pt-12">
      <h1 class="page-title">My account</h1>

      @if (auth.user(); as u) {
        @if (!u.emailConfirmed) {
          <div class="card mt-6 flex flex-wrap items-center justify-between gap-3 bg-saffron-soft p-4">
            <p class="flex items-center gap-2 font-medium"><app-icon name="mail" /> Confirm your email address to secure your account.</p>
            <button class="btn btn-light btn-sm" [disabled]="resent()" (click)="resend()">{{ resent() ? 'Email sent' : 'Resend email' }}</button>
          </div>
        }
      }

      <div class="mt-8 grid items-start gap-8 lg:grid-cols-[220px_1fr]">
        <nav class="flex gap-2 overflow-x-auto lg:flex-col" aria-label="Account sections">
          @for (t of tabs; track t.id) {
            <button class="chip justify-start whitespace-nowrap !px-4 !py-2.5" [attr.aria-pressed]="tab() === t.id" (click)="tab.set(t.id)"><app-icon [name]="t.icon" size="1.1rem" /> {{ t.label }}</button>
          }
        </nav>

        <div>
          @switch (tab()) {
            @case ('profile') {
              <form class="card grid gap-4 p-6 sm:grid-cols-2" [formGroup]="profileForm" (ngSubmit)="saveProfile()">
                <h2 class="text-2xl font-bold sm:col-span-2">Your details</h2>
                <div><label class="label" for="fn">First name</label><input id="fn" class="field" formControlName="firstName" /></div>
                <div><label class="label" for="ln">Last name</label><input id="ln" class="field" formControlName="lastName" /></div>
                <div><label class="label" for="ph">Phone</label><input id="ph" class="field" formControlName="phone" />
                  @if (profileForm.controls.phone.invalid) { <p class="hint-error">10 to 15 digits, optional + at the start.</p> }</div>
                <div><label class="label" for="em">Email</label><input id="em" class="field bg-paper" [value]="me()?.email ?? ''" disabled /></div>
                <div class="sm:col-span-2"><button class="btn btn-primary" [disabled]="profileForm.invalid || busy()">{{ busy() ? 'Saving…' : 'Save changes' }}</button></div>
              </form>
            }
            @case ('addresses') {
              <div class="space-y-4">
                <form class="card grid gap-4 p-6 sm:grid-cols-2" [formGroup]="addressForm" (ngSubmit)="saveAddress()">
                  <h2 class="text-2xl font-bold sm:col-span-2">{{ editingId() ? 'Edit address' : 'Add an address' }}</h2>
                  <div><label class="label" for="city">City</label><input id="city" class="field" formControlName="city" /></div>
                  <div><label class="label" for="street">Street and building</label><input id="street" class="field" formControlName="street" /></div>
                  <div class="sm:col-span-2"><label class="label" for="loc">Google Maps link (optional)</label><input id="loc" class="field" formControlName="locationUrl" placeholder="https://maps.google.com/…" />
                    @if (addressForm.controls.locationUrl.invalid) { <p class="hint-error">Paste a full link starting with https://</p> }</div>
                  <div class="flex gap-2 sm:col-span-2">
                    <button class="btn btn-primary" [disabled]="addressForm.invalid || busy()">{{ editingId() ? 'Update address' : 'Save address' }}</button>
                    @if (editingId()) { <button type="button" class="btn btn-light" (click)="cancelEdit()">Cancel</button> }
                  </div>
                </form>

                @for (a of addresses(); track a.id) {
                  <article class="card-flat flex items-start justify-between gap-3 p-5">
                    <div class="flex gap-3"><app-icon name="pin" class="mt-0.5 text-cobalt" /><div><p class="font-display font-bold">{{ a.city }}</p><p class="text-ink-soft">{{ a.street }}</p></div></div>
                    <div class="flex gap-1">
                      <button class="btn btn-flat btn-sm" (click)="edit(a)" [attr.aria-label]="'Edit address in ' + a.city"><app-icon name="edit" size="1.05rem" /></button>
                      <button class="btn btn-flat btn-sm text-coral" (click)="removeAddress(a)" [attr.aria-label]="'Delete address in ' + a.city"><app-icon name="trash" size="1.05rem" /></button>
                    </div>
                  </article>
                } @empty { <app-empty icon="pin" title="No saved addresses" text="Add one above and it will be ready at checkout." /> }
              </div>
            }
            @case ('security') {
              <form class="card grid max-w-xl gap-4 p-6" [formGroup]="passwordForm" (ngSubmit)="changePassword()">
                <h2 class="text-2xl font-bold">Change password</h2>
                <div><label class="label" for="cp">Current password</label><input id="cp" type="password" class="field" formControlName="currentPassword" autocomplete="current-password" /></div>
                <div><label class="label" for="np">New password</label><input id="np" type="password" class="field" formControlName="newPassword" autocomplete="new-password" />
                  @if (passwordForm.controls.newPassword.touched && passwordForm.controls.newPassword.invalid) { <p class="hint-error">8+ characters with upper, lower case and a number.</p> }</div>
                <div><label class="label" for="cnp">Confirm new password</label><input id="cnp" type="password" class="field" formControlName="confirmNewPassword" autocomplete="new-password" />
                  @if (passwordForm.controls.confirmNewPassword.touched && passwordForm.hasError('mismatch')) { <p class="hint-error">Passwords do not match.</p> }</div>
                <div><button class="btn btn-primary" [disabled]="passwordForm.invalid || busy()">Update password</button></div>
                <hr class="border-line" />
                <button type="button" class="btn btn-light w-fit" (click)="signOutEverywhere()">Sign out on all devices</button>
              </form>
            }
          }
        </div>
      </div>
    </section>
  `,
})
export class AccountComponent {
  auth = inject(AuthService);
  private api = inject(ApiService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  tabs: { id: Tab; label: string; icon: string }[] = [
    { id: 'profile', label: 'Profile', icon: 'user' },
    { id: 'addresses', label: 'Addresses', icon: 'pin' },
    { id: 'security', label: 'Password', icon: 'shield' },
  ];
  tab = signal<Tab>('profile');
  busy = signal(false);
  resent = signal(false);
  me = signal<User | null>(null);
  addresses = signal<Address[]>([]);
  editingId = signal<number | null>(null);

  profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    phone: ['', [Validators.required, Validators.pattern(/^\+?[0-9]{10,15}$/)]],
  });
  addressForm = this.fb.nonNullable.group({
    city: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    street: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    locationUrl: ['', [Validators.pattern(/^$|^https?:\/\/.+/i), Validators.maxLength(500)]],
  });
  passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, strongPassword]],
    confirmNewPassword: ['', Validators.required],
  }, { validators: matches('newPassword', 'confirmNewPassword') });

  constructor() {
    this.auth.me().subscribe({ next: u => { this.me.set(u); this.profileForm.patchValue(u); }, error: () => {} });
    this.loadAddresses();
  }

  private loadAddresses() { this.api.get<Address[]>('Addresses').subscribe({ next: a => this.addresses.set(a), error: () => {} }); }

  saveProfile() {
    this.busy.set(true);
    this.auth.updateProfile(this.profileForm.getRawValue()).subscribe({
      next: u => { this.me.set(u); this.busy.set(false); this.toast.success('Profile updated'); },
      error: e => { this.busy.set(false); this.toast.error(errorMessage(e)); },
    });
  }

  saveAddress() {
    const v = this.addressForm.getRawValue();
    const body = { city: v.city.trim(), street: v.street.trim(), locationUrl: v.locationUrl.trim() || null };
    const id = this.editingId();
    this.busy.set(true);
    (id ? this.api.put<Address>(`Addresses/${id}`, body) : this.api.post<Address>('Addresses', body)).subscribe({
      next: () => { this.busy.set(false); this.cancelEdit(); this.loadAddresses(); this.toast.success(id ? 'Address updated' : 'Address saved'); },
      error: e => { this.busy.set(false); this.toast.error(errorMessage(e)); },
    });
  }
  edit(a: Address) { this.editingId.set(a.id); this.addressForm.patchValue({ city: a.city, street: a.street, locationUrl: a.locationUrl ?? '' }); window.scrollTo({ top: 0, behavior: 'smooth' }); }
  cancelEdit() { this.editingId.set(null); this.addressForm.reset(); }
  removeAddress(a: Address) {
    if (!confirm(`Delete the address in ${a.city}?`)) return;
    this.api.delete<string>(`Addresses/${a.id}`).subscribe({ next: () => { this.loadAddresses(); this.toast.success('Address deleted'); }, error: e => this.toast.error(errorMessage(e)) });
  }

  changePassword() {
    this.busy.set(true);
    this.auth.changePassword(this.passwordForm.getRawValue()).subscribe({
      next: () => { this.busy.set(false); this.passwordForm.reset(); this.toast.success('Password updated'); },
      error: e => { this.busy.set(false); this.toast.error(errorMessage(e)); },
    });
  }

  signOutEverywhere() {
    this.api.post<unknown>('Auth/logout-all').subscribe({ next: () => { this.auth.clear(); location.assign('/login'); }, error: e => this.toast.error(errorMessage(e)) });
  }

  resend() {
    const email = this.auth.user()?.email; if (!email) return;
    this.auth.resendConfirmation(email).subscribe({ next: () => { this.resent.set(true); this.toast.success('Confirmation email sent'); }, error: e => this.toast.error(errorMessage(e)) });
  }
}
