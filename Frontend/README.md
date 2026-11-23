# Mazaj — storefront & admin (Angular 19 + Tailwind CSS 4)

Frontend for the **E-Commerce** ASP.NET API in this repository. Brand persona: *Mazaj* ("mood") — a playful
store where shoppers pick a mood (a category) and the page re-colours around it.

## Run it

```bash
cd Frontend
npm install

npm start            # talks to the real API at http://localhost:5080
npm run start:demo   # no backend needed: in-browser mock API (great for client demos)
```

Open http://localhost:4200.

**Demo mode:** sign in with any email and password. An email containing `admin` (e.g. `admin@mazaj.demo`)
gets the admin dashboard. Data is kept in `localStorage` (key `mazaj.mock.v1`) so a refresh keeps your session.
Coupons to try: `WELCOME10`, `SAVE50`.

## Connect to the real backend

1. Start the API (`dotnet run` in `E-Commerce/`, default `http://localhost:5080`). CORS already allows `http://localhost:4200`.
2. Change the API URL in `src/environments/environment.ts` (`apiUrl`) for other environments.
3. In the API's `appsettings*.json` set `Auth:FrontendUrl` to `http://localhost:4200`
   (it builds the e-mail links `/confirm-email` and `/reset-password`, and the Paymob return URL
   `/orders/:id/payment-result`).
4. Add your production frontend origin to `Cors:AllowedOrigins`.

## Build

```bash
npm run build        # production bundle in dist/mazaj/browser
npm run build:demo   # static demo bundle with the mock API (deployable to Netlify/Vercel/GitHub Pages)
```

For static hosting, add an SPA fallback so every path serves `index.html`.

## What is inside

| Area | Pages |
| --- | --- |
| Storefront | Home (mood picker), Shop (search, mood, price, sort, pagination), Product (gallery, reviews, related), Basket (coupon), Checkout (addresses, card / wallet / cash), Payment result, Orders + tracking + cancel, Favourites, Account (profile, addresses, password) |
| Auth | Login, Register, Forgot / Reset password, Confirm e-mail, silent token refresh |
| Admin | Dashboard (KPIs, revenue chart, best sellers, low stock), Products (+ image upload), Categories, Orders (status), Coupons, Offers |

## Code map

- `src/app/core` — models mirroring the API DTOs, `ApiService`, auth (JWT + refresh), cart / wishlist signals, guards, interceptors, `mock-api.ts`.
- `src/app/shared` — icons, product card, stars, price, pagination, empty states.
- `src/app/layout` — storefront shell (header, basket drawer, footer) and admin shell.
- `src/app/pages` — one lazy-loaded component per route.

## Re-branding

All colours, fonts and shadows are design tokens in `src/styles.css` (`@theme`). The wordmark is in
`src/app/layout/logo.component.ts`; the mood colours per category are in `src/app/core/utils.ts`.
