import { Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './core/guards';
import { ShellComponent } from './layout/shell.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', title: 'Mazaj — Shop your mood', loadComponent: () => import('./pages/home.component').then(m => m.HomeComponent) },
      { path: 'shop', title: 'Shop — Mazaj', loadComponent: () => import('./pages/shop.component').then(m => m.ShopComponent) },
      { path: 'category/:id', title: 'Shop — Mazaj', loadComponent: () => import('./pages/shop.component').then(m => m.ShopComponent) },
      { path: 'product/:id', title: 'Product — Mazaj', loadComponent: () => import('./pages/product.component').then(m => m.ProductComponent) },
      { path: 'cart', title: 'Basket — Mazaj', canActivate: [authGuard], loadComponent: () => import('./pages/cart.component').then(m => m.CartComponent) },
      { path: 'checkout', title: 'Checkout — Mazaj', canActivate: [authGuard], loadComponent: () => import('./pages/checkout.component').then(m => m.CheckoutComponent) },
      { path: 'orders', title: 'Your orders — Mazaj', canActivate: [authGuard], loadComponent: () => import('./pages/orders.component').then(m => m.OrdersComponent) },
      { path: 'orders/:id', title: 'Order — Mazaj', canActivate: [authGuard], loadComponent: () => import('./pages/orders.component').then(m => m.OrderDetailComponent) },
      { path: 'orders/:id/payment-result', title: 'Payment — Mazaj', canActivate: [authGuard], loadComponent: () => import('./pages/orders.component').then(m => m.PaymentResultComponent) },
      { path: 'wishlist', title: 'Favourites — Mazaj', canActivate: [authGuard], loadComponent: () => import('./pages/wishlist.component').then(m => m.WishlistComponent) },
      { path: 'account', title: 'My account — Mazaj', canActivate: [authGuard], loadComponent: () => import('./pages/account.component').then(m => m.AccountComponent) },
      { path: 'login', title: 'Sign in — Mazaj', canActivate: [guestGuard], loadComponent: () => import('./pages/auth.components').then(m => m.LoginComponent) },
      { path: 'register', title: 'Join — Mazaj', canActivate: [guestGuard], loadComponent: () => import('./pages/auth.components').then(m => m.RegisterComponent) },
      { path: 'forgot-password', title: 'Forgot password — Mazaj', loadComponent: () => import('./pages/auth.components').then(m => m.ForgotPasswordComponent) },
      { path: 'reset-password', title: 'Reset password — Mazaj', loadComponent: () => import('./pages/auth.components').then(m => m.ResetPasswordComponent) },
      { path: 'confirm-email', title: 'Confirm email — Mazaj', loadComponent: () => import('./pages/auth.components').then(m => m.ConfirmEmailComponent) },
    ],
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    title: 'Admin — Mazaj',
    loadComponent: () => import('./layout/admin-shell.component').then(m => m.AdminShellComponent),
    children: [
      { path: '', loadComponent: () => import('./pages/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent) },
      { path: 'products', loadComponent: () => import('./pages/admin/admin-products.component').then(m => m.AdminProductsComponent) },
      { path: 'products/:id', loadComponent: () => import('./pages/admin/admin-products.component').then(m => m.AdminProductFormComponent) },
      { path: 'categories', loadComponent: () => import('./pages/admin/admin-catalog.component').then(m => m.AdminCategoriesComponent) },
      { path: 'orders', loadComponent: () => import('./pages/admin/admin-catalog.component').then(m => m.AdminOrdersComponent) },
      { path: 'coupons', loadComponent: () => import('./pages/admin/admin-catalog.component').then(m => m.AdminCouponsComponent) },
      { path: 'offers', loadComponent: () => import('./pages/admin/admin-catalog.component').then(m => m.AdminOffersComponent) },
    ],
  },
  { path: '**', component: ShellComponent, children: [{ path: '', title: 'Not found — Mazaj', loadComponent: () => import('./pages/not-found.component').then(m => m.NotFoundComponent) }] },
];
