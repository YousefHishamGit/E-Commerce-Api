/**
 * In-browser mock of the E-Commerce API, used only by the `demo` build
 * (`npm run start:demo` / `npm run build:demo`) so the storefront can be shown
 * to clients without the .NET backend. Sign in with any email/password —
 * an email containing "admin" gets the Admin role.
 */
import { HttpErrorResponse, HttpEvent, HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable, delay, of, tap, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Address, Basket, Category, Coupon, Offer, Order, Product, Review, User, Wishlist,
} from './models';

type Body = any;
const uid = () => crypto.randomUUID();
const day = 864e5;

/* ───────── Seed data ───────── */
const svgImg = (emoji: string, bg: string) =>
  'data:image/svg+xml;utf8,' + encodeURIComponent(
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 400"><rect width="400" height="400" fill="${bg}"/><circle cx="330" cy="70" r="90" fill="#fff" opacity=".22"/><circle cx="60" cy="350" r="110" fill="#16142e" opacity=".07"/><text x="200" y="250" font-size="170" text-anchor="middle">${emoji}</text></svg>`);

let categories: Category[] = [
  { id: 1, name: 'Focus', description: 'Desk gear for deep work.', productCount: 0 },
  { id: 2, name: 'Cozy', description: 'Soft things for slow evenings.', productCount: 0 },
  { id: 3, name: 'Active', description: 'Move more, worry less.', productCount: 0 },
  { id: 4, name: 'Glow', description: 'Small rituals for you.', productCount: 0 },
  { id: 5, name: 'Wander', description: 'Pack light, go far.', productCount: 0 },
];

const P = (id: number, name: string, price: number, cat: number, emoji: string, bg: string, stock: number, desc: string, rating: number, reviews: number): Product => ({
  id, name, price, categoryId: cat, categoryName: '', photo: svgImg(emoji, bg), images: [{ id: id * 10, url: svgImg(emoji, bg), isMain: true }],
  description: desc, stockQuantity: stock, averageRating: rating, reviewsCount: reviews, hasOffer: false, createdAt: new Date(Date.now() - id * day).toISOString(),
});

let products: Product[] = [
  P(1, 'Quiet Wireless Headphones', 1850, 1, '🎧', '#e3e6ff', 14, 'Over-ear headphones with soft noise cancelling and a 30-hour battery. Folds flat for your bag.', 4.6, 12),
  P(2, 'Mechanical Keyboard 75%', 2400, 1, '⌨️', '#cdf6ea', 8, 'Hot-swappable switches, PBT keycaps and a gasket mount that sounds as good as it types.', 4.8, 21),
  P(3, 'Brass Desk Lamp', 780, 1, '💡', '#fff0bd', 22, 'A warm, dimmable lamp with a weighted base. Adjusts to any angle.', 4.2, 6),
  P(4, 'Chunky Knit Throw', 690, 2, '🧣', '#ebe5ff', 30, 'Oversized, soft and heavy enough to feel like a hug. Machine washable.', 4.7, 18),
  P(5, 'Soy Candle — Cedar & Fig', 320, 2, '🕯️', '#ffe0e1', 3, 'Hand-poured in Cairo. Burns clean for about 45 hours.', 4.5, 9),
  P(6, 'Ceramic Mug Set (4)', 540, 2, '☕', '#fff0bd', 18, 'Four glazed mugs in sunset colours. Dishwasher and microwave safe.', 4.4, 7),
  P(7, 'Everyday Running Shoes', 2100, 3, '👟', '#cdf6ea', 25, 'Lightweight and springy. Breathable mesh upper keeps feet cool.', 4.3, 15),
  P(8, 'Insulated Steel Bottle', 410, 3, '🥤', '#e3e6ff', 40, 'Keeps drinks cold 24 hours or hot for 12. Fits most cup holders.', 4.6, 11),
  P(9, 'Resistance Band Trio', 260, 3, '🏋️', '#ffe0e1', 0, 'Three strengths for stretching, rehab and strength work.', 4.1, 4),
  P(10, 'Vitamin C Glow Serum', 480, 4, '🧴', '#fff0bd', 16, 'Brightening serum with 10% vitamin C. Gentle on sensitive skin.', 4.5, 14),
  P(11, 'Silk Sleep Mask', 190, 4, '😴', '#ebe5ff', 35, 'Pure mulberry silk that blocks light and never tugs on your lashes.', 4.7, 10),
  P(12, 'Rose Clay Face Mask', 230, 4, '🌹', '#ffe0e1', 12, 'A weekly reset. Draws out impurities without drying your skin.', 4.0, 5),
  P(13, 'Weekender Backpack', 1650, 5, '🎒', '#cdf6ea', 9, 'Water-resistant 28L bag with a laptop sleeve and a hidden passport pocket.', 4.6, 17),
  P(14, 'Packing Cubes (6)', 340, 5, '🧳', '#e3e6ff', 27, 'Compress and sort your clothes. Mesh tops let you see what is inside.', 4.4, 8),
  P(15, 'Compact Travel Adapter', 290, 5, '🔌', '#fff0bd', 4, 'One plug for 150+ countries. Two USB-C ports, two USB-A.', 4.3, 6),
];

let reviews: Review[] = [
  { id: 1, rating: 5, comment: 'Best purchase this year. The battery really lasts.', isVerifiedPurchase: true, userId: 'u-seed', userFullName: 'Nour Hassan', productId: 1, createdAt: new Date(Date.now() - 6 * day).toISOString() },
  { id: 2, rating: 4, comment: 'Comfortable for long calls. Case could be sturdier.', isVerifiedPurchase: true, userId: 'u-seed2', userFullName: 'Omar Fathy', productId: 1, createdAt: new Date(Date.now() - 12 * day).toISOString() },
  { id: 3, rating: 5, comment: 'Sounds like a typewriter in the best way.', isVerifiedPurchase: false, userId: 'u-seed', userFullName: 'Nour Hassan', productId: 2, createdAt: new Date(Date.now() - 3 * day).toISOString() },
];

let offers: Offer[] = [
  { id: 1, title: 'Cozy season', description: 'Everything cozy, a little cheaper.', discountPercentage: 20, startDate: new Date(Date.now() - 2 * day).toISOString(), endDate: new Date(Date.now() + 9 * day).toISOString(), isActive: true, categoryId: 2, categoryName: 'Cozy' },
  { id: 2, title: 'Keyboard week', description: '', discountPercentage: 15, startDate: new Date(Date.now() - day).toISOString(), endDate: new Date(Date.now() + 5 * day).toISOString(), isActive: true, productId: 2, productName: 'Mechanical Keyboard 75%' },
  { id: 3, title: 'Run club', description: '', discountPercentage: 10, startDate: new Date(Date.now() - day).toISOString(), endDate: new Date(Date.now() + 12 * day).toISOString(), isActive: true, categoryId: 3, categoryName: 'Active' },
];

let coupons: Coupon[] = [
  { id: 1, code: 'WELCOME10', type: 'Percentage', value: 10, minOrderAmount: 200, startDate: new Date(Date.now() - 30 * day).toISOString(), endDate: new Date(Date.now() + 90 * day).toISOString(), maxUsage: null, usedCount: 12, isActive: true },
  { id: 2, code: 'SAVE50', type: 'FixedAmount', value: 50, minOrderAmount: 500, startDate: new Date(Date.now() - 30 * day).toISOString(), endDate: new Date(Date.now() + 30 * day).toISOString(), maxUsage: 100, usedCount: 31, isActive: true },
];

let users: (User & { password: string })[] = [];
let addresses: Address[] = [{ id: 1, city: 'Cairo', street: '12 Tahrir St, apt 4', locationUrl: null }];
let nextId = { address: 2, product: 100, category: 100, review: 100, coupon: 100, offer: 100, image: 1000 };
let wishlistIds = new Set<number>();
let basket: { productId: number; quantity: number }[] = [];
let basketCoupon: string | null = null;
let orders: Order[] = [];

const seedOrders = () => {
  const names = ['Nour Hassan', 'Omar Fathy', 'Salma Adel', 'Karim Samir'];
  const statuses: Order['status'][] = ['Delivered', 'Delivered', 'Shipped', 'Confirmed', 'Pending', 'Delivered', 'Cancelled', 'Delivered'];
  statuses.forEach((status, i) => {
    const prod = products[(i * 3) % products.length];
    const qty = 1 + (i % 3);
    const sub = prod.price * qty;
    orders.push({
      id: uid(), status, subTotal: sub, discountAmount: 0, shippingCost: sub >= 1000 ? 0 : 50, taxAmount: Math.round(sub * 0.14), totalPrice: sub + Math.round(sub * 0.14) + (sub >= 1000 ? 0 : 50),
      customerId: 'u-' + i, customerName: names[i % names.length], shippingCity: 'Cairo', shippingStreet: `${10 + i} Nile Corniche`, shippingLocationUrl: null,
      items: [{ id: i + 1, productId: prod.id, productName: prod.name, productPhoto: prod.photo, unitPrice: prod.price, quantity: qty, subTotal: sub }],
      payments: [{ id: i + 1, method: i % 2 ? 'CashOnDelivery' : 'CreditCard', status: status === 'Delivered' || status === 'Shipped' ? 'Completed' : 'Pending', amount: sub, orderId: '', paidAt: null }],
      createdAt: new Date(Date.now() - (i * 3 + 1) * day / 2).toISOString(),
    });
    orders[i].payments[0].orderId = orders[i].id;
  });
};
seedOrders();

/* ───────── Persistence (so a page refresh keeps the demo session) ───────── */
const STORE_KEY = 'mazaj.mock.v1';
const snapshot = () => ({ categories, products, reviews, offers, coupons, users, addresses, wishlist: [...wishlistIds], basket, basketCoupon, orders, nextId });
try {
  const raw = localStorage.getItem(STORE_KEY);
  if (raw) {
    const d = JSON.parse(raw);
    ({ categories, products, reviews, offers, coupons, users, addresses, basket, basketCoupon, orders, nextId } = d);
    wishlistIds = new Set<number>(d.wishlist);
  }
} catch { /* ignore corrupt demo state */ }
const persist = () => { try { localStorage.setItem(STORE_KEY, JSON.stringify(snapshot())); } catch { /* quota or private mode */ } };

/* ───────── Helpers ───────── */
const activeOffer = (p: Product) => offers.find(o => o.isActive && new Date(o.endDate) > new Date() && new Date(o.startDate) <= new Date() && (o.productId === p.id || (!o.productId && o.categoryId === p.categoryId)));
const view = (p: Product): Product => {
  const o = activeOffer(p);
  return { ...p, categoryName: categories.find(c => c.id === p.categoryId)?.name ?? '', hasOffer: !!o, offerTitle: o?.title ?? null, discountPercentage: o?.discountPercentage ?? null,
    priceAfterDiscount: o ? Math.round(p.price * (1 - o.discountPercentage / 100) * 100) / 100 : null };
};
const unitPrice = (p: Product) => view(p).priceAfterDiscount ?? p.price;

const buildBasket = (): Basket => {
  const items = basket.map(b => { const p = products.find(x => x.id === b.productId)!; return { id: p.id, productName: p.name, pictureUrl: p.photo ?? '', price: unitPrice(p), quantity: b.quantity }; });
  const subtotal = items.reduce((n, i) => n + i.price * i.quantity, 0);
  const c = basketCoupon ? coupons.find(x => x.code === basketCoupon) : null;
  const couponDiscount = c ? (c.type === 'Percentage' ? Math.round(subtotal * c.value) / 100 : Math.min(c.value, subtotal)) : 0;
  const shipping = items.length ? (subtotal >= 1000 ? 0 : 50) : 0;
  const tax = Math.round((subtotal - couponDiscount) * 0.14 * 100) / 100;
  return { id: 'demo', items, couponCode: c?.code ?? null, couponDiscount: couponDiscount || null, offerDiscount: null, shippingCost: shipping, taxesCost: tax, totalPrice: Math.round((subtotal - couponDiscount + shipping + tax) * 100) / 100 };
};

const ok = (data: unknown, status = 200) => ({ success: true, data, statusCode: status });
const fail = (status: number, message: string) => throwError(() => new HttpErrorResponse({ status, error: { success: false, message, statusCode: status } }));
const page = <T>(list: T[], q: URLSearchParams, size = 10) => {
  const pageSize = Math.min(+(q.get('pageSize') ?? size), 50), pageIndex = Math.max(1, +(q.get('pageIndex') ?? 1));
  const totalPages = Math.max(1, Math.ceil(list.length / pageSize));
  return { data: list.slice((pageIndex - 1) * pageSize, pageIndex * pageSize), totalCount: list.length, pageIndex, pageSize, totalPages, hasNextPage: pageIndex < totalPages, hasPreviousPage: pageIndex > 1 };
};

const session = (u: User & { password?: string }) => ({
  userId: u.id, email: u.email, fullName: `${u.firstName} ${u.lastName}`, role: u.role, emailConfirmed: u.emailConfirmed,
  token: 'mock.' + u.id, tokenExpiry: new Date(Date.now() + 36e5).toISOString(), refreshToken: 'r.' + u.id, refreshTokenExpiry: new Date(Date.now() + 7 * day).toISOString(),
});
const currentUser = (req: HttpRequest<unknown>) => users.find(u => 'mock.' + u.id === req.headers.get('Authorization')?.replace('Bearer ', ''));

/* ───────── Router ───────── */
function handle(req: HttpRequest<unknown>): Observable<HttpResponse<unknown>> | null {
  const url = new URL(req.url);
  const path = url.pathname.replace(/^\/api\//i, '').replace(/\/$/, '');
  const q = url.searchParams, m = req.method, b = (req.body ?? {}) as Body;
  const seg = path.split('/'), [res, a1, a2, a3] = seg;
  const user = currentUser(req);
  const needAuth = () => (user ? null : fail(401, 'Please sign in.'));
  const needAdmin = () => (user?.role === 'Admin' ? null : fail(user ? 403 : 401, 'Admins only.'));
  const send = (data: unknown, status = 200) => of(new HttpResponse({ status, body: ok(data, status) }));

  switch (res.toLowerCase()) {
    case 'auth': {
      if (a1 === 'login' && m === 'POST') {
        let u = users.find(x => x.email === b.email);
        if (!u) { const name = (b.email as string).split('@')[0]; u = mkUser(name.charAt(0).toUpperCase() + name.slice(1), 'Demo', b.email, '01000000000'); }
        return send(session(u));
      }
      if (a1 === 'register' && m === 'POST') {
        if (users.some(x => x.email === b.email)) return fail(409, 'An account with this email already exists.');
        return send(session(mkUser(b.firstName, b.lastName, b.email, b.phone, b.gender)), 201);
      }
      if (a1 === 'refresh') { const u = users.find(x => 'r.' + x.id === b.refreshToken); return u ? send(session(u)) : fail(401, 'Session expired.'); }
      if (['logout', 'logout-all', 'forgot-password', 'reset-password', 'confirm-email', 'resend-confirmation', 'change-password'].includes(a1)) {
        if (a1 === 'confirm-email' && user) user.emailConfirmed = true;
        return send(null);
      }
      if (a1 === 'me') {
        const e = needAuth(); if (e) return e;
        if (m === 'PUT') Object.assign(user!, Object.fromEntries(Object.entries(b).filter(([, v]) => v != null)));
        return send(strip(user!));
      }
      break;
    }

    case 'categories': {
      const withCount = categories.map(c => ({ ...c, productCount: products.filter(p => p.categoryId === c.id).length }));
      if (m === 'GET') return a1 ? send(withCount.find(c => c.id === +a1)) : send(withCount);
      const e = needAdmin(); if (e) return e;
      if (m === 'POST') { const c = { id: nextId.category++, name: b.name, description: b.description, productCount: 0 }; categories.push(c); return send(c, 201); }
      if (m === 'PUT') { const c = categories.find(x => x.id === +a1)!; Object.assign(c, b); return send(c); }
      if (m === 'DELETE') { categories = categories.filter(c => c.id !== +a1); return send('Deleted'); }
      break;
    }

    case 'products': {
      if (m === 'GET' && !a1) {
        let list = products.map(view);
        const cat = q.get('CategoryId') ?? q.get('categoryId'); if (cat) list = list.filter(p => p.categoryId === +cat);
        const s = (q.get('search') ?? '').toLowerCase(); if (s) list = list.filter(p => p.name.toLowerCase().includes(s) || (p.description ?? '').toLowerCase().includes(s));
        const min = q.get('minPrice'), max = q.get('maxPrice');
        if (min) list = list.filter(p => (p.priceAfterDiscount ?? p.price) >= +min); if (max) list = list.filter(p => (p.priceAfterDiscount ?? p.price) <= +max);
        const price = (p: Product) => p.priceAfterDiscount ?? p.price;
        const sort = q.get('sort');
        list.sort(sort === 'price_asc' ? (x, y) => price(x) - price(y) : sort === 'price_desc' ? (x, y) => price(y) - price(x) : sort === 'rating' ? (x, y) => y.averageRating - x.averageRating : sort === 'name_desc' ? (x, y) => y.name.localeCompare(x.name) : (x, y) => +new Date(y.createdAt) - +new Date(x.createdAt));
        return send(page(list, q, 12));
      }
      if (m === 'GET') { const p = products.find(x => x.id === +a1); return p ? send(view(p)) : fail(404, 'Product not found.'); }
      const e = needAdmin(); if (e) return e;
      if (m === 'POST' && !a1) { const p = { ...P(nextId.product++, b.name, b.price, b.categoryId, '🎁', '#e3e6ff', b.stockQuantity, b.description ?? '', 0, 0), photo: null, images: [] }; products.unshift(p); return send(view(p), 201); }
      if (m === 'PUT') { const p = products.find(x => x.id === +a1)!; Object.assign(p, b); return send(view(p)); }
      if (m === 'DELETE' && !a2) { products = products.filter(p => p.id !== +a1); return send('Deleted'); }
      if (a2 === 'images' && m === 'POST') {
        const p = products.find(x => x.id === +a1)!; const files = (req.body as FormData).getAll('files') as File[];
        files.forEach(f => p.images.push({ id: nextId.image++, url: URL.createObjectURL(f), isMain: p.images.length === 0 }));
        p.photo = p.images[0]?.url ?? null; return send(p.images);
      }
      if (a2 === 'images' && m === 'DELETE') { const p = products.find(x => x.id === +a1)!; p.images = p.images.filter(i => i.id !== +a3); if (p.images[0]) p.images[0].isMain = true; p.photo = p.images[0]?.url ?? null; return send(p.images); }
      break;
    }

    case 'reviews': {
      if (a1 === 'product') return send(page(reviews.filter(r => r.productId === +a2).sort((x, y) => +new Date(y.createdAt) - +new Date(x.createdAt)), q));
      const e = needAuth(); if (e) return e;
      if (m === 'POST') {
        const r: Review = { id: nextId.review++, rating: b.rating, comment: b.comment, isVerifiedPurchase: orders.some(o => o.items.some(i => i.productId === b.productId)), userId: user!.id, userFullName: user!.fullName, productId: b.productId, createdAt: new Date().toISOString() };
        reviews.push(r); recalc(b.productId); return send(r, 201);
      }
      if (m === 'DELETE') { const r = reviews.find(x => x.id === +a1); reviews = reviews.filter(x => x.id !== +a1); if (r) recalc(r.productId); return send('Deleted'); }
      break;
    }

    case 'basket': {
      const e = needAuth(); if (e) return e;
      if (a1 === 'items' && m === 'POST') { const row = basket.find(x => x.productId === b.productId); row ? (row.quantity += b.quantity) : basket.push({ productId: b.productId, quantity: b.quantity }); return send(buildBasket()); }
      if (a1 === 'items' && m === 'PUT') { const row = basket.find(x => x.productId === +a2); if (row) row.quantity = b.quantity; return send(buildBasket()); }
      if (a1 === 'items' && m === 'DELETE') { basket = basket.filter(x => x.productId !== +a2); return send(buildBasket()); }
      if (a1 === 'coupon' && m === 'POST') {
        const c = coupons.find(x => x.code === String(b.code).toUpperCase() && x.isActive);
        if (!c) return fail(400, 'That coupon code is not valid.');
        if (c.minOrderAmount && buildBasket().items.reduce((n, i) => n + i.price * i.quantity, 0) < c.minOrderAmount) return fail(400, `This coupon needs an order of at least EGP ${c.minOrderAmount}.`);
        basketCoupon = c.code; return send(buildBasket());
      }
      if (a1 === 'coupon' && m === 'DELETE') { basketCoupon = null; return send(buildBasket()); }
      if (m === 'DELETE') { basket = []; basketCoupon = null; return send('Cleared'); }
      return send(buildBasket());
    }

    case 'wishlist': {
      const e = needAuth(); if (e) return e;
      if (m === 'POST') wishlistIds.add(b.productId);
      if (m === 'DELETE' && a1) wishlistIds.delete(+a1);
      const w: Wishlist = { id: 1, userId: user!.id, totalItems: wishlistIds.size, items: [...wishlistIds].map(id => { const p = products.find(x => x.id === id)!; return { id, productId: id, productName: p.name, productPrice: unitPrice(p), productPhoto: p.photo, stockQuantity: p.stockQuantity }; }) };
      return send(w);
    }

    case 'addresses': {
      const e = needAuth(); if (e) return e;
      if (m === 'GET') return send(addresses);
      if (m === 'POST') { const a = { id: nextId.address++, city: b.city, street: b.street, locationUrl: b.locationUrl }; addresses.push(a); return send(a, 201); }
      if (m === 'PUT') { const a = addresses.find(x => x.id === +a1)!; Object.assign(a, b); return send(a); }
      if (m === 'DELETE') { addresses = addresses.filter(x => x.id !== +a1); return send('Deleted'); }
      break;
    }

    case 'orders': {
      const e = needAuth(); if (e) return e;
      if (a1 === 'all') { const e2 = needAdmin(); if (e2) return e2; return send(page([...orders].sort((x, y) => +new Date(y.createdAt) - +new Date(x.createdAt)), q, 15)); }
      if (m === 'POST' && !a1) {
        const bk = buildBasket(); if (!bk.items.length) return fail(400, 'Your basket is empty.');
        const addr = addresses.find(x => x.id === b.deliveryAddressId); if (!addr) return fail(400, 'Choose a delivery address.');
        const id = uid(); const sub = bk.items.reduce((n, i) => n + i.price * i.quantity, 0);
        const o: Order = {
          id, status: 'Pending', subTotal: sub, discountAmount: bk.couponDiscount ?? 0, shippingCost: bk.shippingCost ?? 0, taxAmount: bk.taxesCost ?? 0, totalPrice: bk.totalPrice ?? sub, couponCode: bk.couponCode,
          customerId: user!.id, customerName: user!.fullName, shippingCity: addr.city, shippingStreet: addr.street, shippingLocationUrl: addr.locationUrl,
          items: bk.items.map((i, n) => ({ id: n + 1, productId: i.id, productName: i.productName, productPhoto: i.pictureUrl, unitPrice: i.price, quantity: i.quantity, subTotal: i.price * i.quantity })),
          payments: [{ id: Date.now() % 100000, method: b.paymentMethod, status: 'Pending', amount: bk.totalPrice ?? sub, orderId: id }], createdAt: new Date().toISOString(),
        };
        orders.unshift(o); basket = []; basketCoupon = null; return send(o, 201);
      }
      if (!a1 && m === 'GET') return send(orders.filter(o => o.customerId === user!.id));
      const o = orders.find(x => x.id === a1);
      if (!o) return fail(404, 'Order not found.');
      if (a2 === 'status' && m === 'PATCH') { o.status = b.status; return send(o); }
      if (a2 === 'cancel') { if (o.status !== 'Pending') return fail(400, 'Only pending orders can be cancelled.'); o.status = 'Cancelled'; return send(o); }
      return send(o);
    }

    case 'payments': {
      const e = needAuth(); if (e) return e;
      if (a1 === 'orders' && a3 === 'pay') {
        const o = orders.find(x => x.id === a2)!; o.payments[0].status = 'Completed'; o.payments[0].paidAt = new Date().toISOString(); o.status = 'Confirmed';
        return send({ paymentId: o.payments[0].id, orderId: o.id, amount: o.totalPrice, currency: 'EGP', checkoutUrl: `${location.origin}${location.pathname.replace(/\/[^/]*$/, '')}/orders/${o.id}/payment-result`.replace(/([^:])\/\//g, '$1/') });
      }
      break;
    }

    case 'offers': {
      if (a1 === 'active') return send(offers.filter(o => o.isActive && new Date(o.endDate) > new Date()));
      if (a1 === 'product') return send(activeOffer(products.find(p => p.id === +a2)!) ?? null);
      const e = needAdmin(); if (e) return e;
      if (m === 'GET') return send(offers);
      if (m === 'POST' && !a1) { const o = { ...b, id: nextId.offer++, isActive: true, productName: products.find(p => p.id === b.productId)?.name, categoryName: categories.find(c => c.id === b.categoryId)?.name }; offers.push(o); return send(o, 201); }
      if (m === 'PUT') { const o = offers.find(x => x.id === +a1)!; Object.assign(o, b, { productName: products.find(p => p.id === b.productId)?.name, categoryName: categories.find(c => c.id === b.categoryId)?.name }); return send(o); }
      if (a2 === 'toggle') { const o = offers.find(x => x.id === +a1)!; o.isActive = !o.isActive; return send(o); }
      if (m === 'DELETE') { offers = offers.filter(o => o.id !== +a1); return send('Deleted'); }
      break;
    }

    case 'coupons': {
      if (a1 === 'validate' && m === 'POST') {
        const c = coupons.find(x => x.code === String(b.code).toUpperCase() && x.isActive);
        if (!c) return send({ isValid: false, message: 'Invalid coupon', discountAmount: 0, finalAmount: b.orderAmount });
        const d = c.type === 'Percentage' ? (b.orderAmount * c.value) / 100 : c.value; return send({ isValid: true, discountAmount: d, finalAmount: b.orderAmount - d });
      }
      const e = needAdmin(); if (e) return e;
      if (m === 'GET') return send(coupons);
      if (m === 'POST') { const c = { ...b, id: nextId.coupon++, usedCount: 0, isActive: true }; coupons.push(c); return send(c, 201); }
      if (m === 'PUT') { const c = coupons.find(x => x.id === +a1)!; Object.assign(c, b); return send(c); }
      if (a2 === 'toggle') { const c = coupons.find(x => x.id === +a1)!; c.isActive = !c.isActive; return send(c); }
      if (m === 'DELETE') { coupons = coupons.filter(c => c.id !== +a1); return send('Deleted'); }
      break;
    }

    case 'dashboard': {
      const e = needAdmin(); if (e) return e;
      const live = orders.filter(o => o.status !== 'Cancelled');
      const revenue = live.reduce((n, o) => n + o.totalPrice, 0);
      const today = live.filter(o => new Date(o.createdAt).toDateString() === new Date().toDateString());
      if (a1 === 'summary') return send({ totalCustomers: 128 + users.length, totalProducts: products.length, totalOrders: orders.length, pendingOrders: orders.filter(o => o.status === 'Pending').length, lowStockProducts: products.filter(p => p.stockQuantity > 0 && p.stockQuantity <= 5).length, outOfStockProducts: products.filter(p => p.stockQuantity === 0).length, totalRevenue: revenue, averageOrderValue: live.length ? revenue / live.length : 0, todayOrders: today.length, todayRevenue: today.reduce((n, o) => n + o.totalPrice, 0), newCustomersLast30Days: 23 });
      if (a1 === 'sales') { const days = +(q.get('days') ?? 30); return send(Array.from({ length: days }, (_, i) => { const d = new Date(Date.now() - (days - 1 - i) * day); const base = 1800 + Math.round(1600 * Math.abs(Math.sin(i * 0.9)) + 700 * Math.cos(i * 0.4)); return { date: d.toISOString(), ordersCount: Math.max(1, Math.round(base / 900)), revenue: base }; })); }
      if (a1 === 'top-products') return send(products.slice(0, +(q.get('count') ?? 5)).map((p, i) => ({ productId: p.id, name: p.name, photo: p.photo, quantitySold: 40 - i * 6, revenue: (40 - i * 6) * p.price })));
      if (a1 === 'low-stock') return send(products.filter(p => p.stockQuantity <= 5).map(p => ({ productId: p.id, name: p.name, categoryName: categories.find(c => c.id === p.categoryId)?.name ?? '', stockQuantity: p.stockQuantity })));
      if (a1 === 'orders-by-status') return send(['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'].map(s => ({ status: s, count: orders.filter(o => o.status === s).length })));
      if (a1 === 'recent-orders') return send(orders.slice(0, +(q.get('count') ?? 7)).map(o => ({ orderId: o.id, customerName: o.customerName, status: o.status, totalPrice: o.totalPrice, itemsCount: o.items.length, createdAt: o.createdAt })));
      break;
    }
  }
  return fail(404, `Mock API: ${m} /${path} is not implemented.`) as Observable<never>;
}

function mkUser(first: string, last: string, email: string, phone: string, gender = 'Male') {
  const u = { id: uid(), firstName: first, lastName: last, fullName: `${first} ${last}`, phone, email, gender, role: (email.toLowerCase().includes('admin') ? 'Admin' : 'Customer') as User['role'], emailConfirmed: false, createdAt: new Date().toISOString(), password: '' };
  users.push(u); return u;
}
const strip = (u: User & { password?: string }) => { const { password, ...rest } = u; return rest; };
function recalc(productId: number) {
  const list = reviews.filter(r => r.productId === productId); const p = products.find(x => x.id === productId);
  if (p) { p.reviewsCount = list.length; p.averageRating = list.length ? Math.round((list.reduce((n, r) => n + r.rating, 0) / list.length) * 10) / 10 : 0; }
}

export const mockInterceptor: HttpInterceptorFn = (req, next): Observable<HttpEvent<unknown>> => {
  if (!environment.useMocks || !req.url.startsWith(environment.apiUrl)) return next(req);
  return (handle(req) ?? next(req)).pipe(tap(() => persist()), delay(220));
};
