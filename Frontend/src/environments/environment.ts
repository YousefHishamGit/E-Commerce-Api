export const environment = {
  /** Base URL of the E-Commerce ASP.NET API (no trailing slash). */
  // Use the https URL: the API calls UseHttpsRedirection() before CORS, so http://localhost:5080
  // answers the browser's preflight with a redirect, which browsers reject.
  apiUrl: 'https://localhost:7218',
  /** When true, an in-browser mock API replaces the backend (portfolio demo). */
  useMocks: false,
};
