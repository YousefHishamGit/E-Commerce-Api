/** Brand "moods": each category gets a colour personality, cycled by id. */
export interface Mood {
  /** Solid background class (hero tile, chips). */
  bg: string;
  /** Soft tint background. */
  soft: string;
  /** Text colour that is readable on `bg`. */
  fg: string;
  /** Hex for use in SVG / inline styles. */
  hex: string;
  emoji: string;
}

const MOODS: Mood[] = [
  { bg: 'bg-cobalt', soft: 'bg-cobalt-soft', fg: 'text-white', hex: '#2d3df0', emoji: '⚡' },
  { bg: 'bg-saffron', soft: 'bg-saffron-soft', fg: 'text-ink', hex: '#ffc42e', emoji: '☀️' },
  { bg: 'bg-coral', soft: 'bg-coral-soft', fg: 'text-white', hex: '#ff5b5f', emoji: '🔥' },
  { bg: 'bg-mint', soft: 'bg-mint-soft', fg: 'text-ink', hex: '#14c49a', emoji: '🌿' },
  { bg: 'bg-lilac', soft: 'bg-lilac-soft', fg: 'text-ink', hex: '#b8a6ff', emoji: '🌙' },
];

export function moodFor(id: number | null | undefined): Mood {
  if (id == null) return { ...MOODS[0], bg: 'bg-ink', soft: 'bg-white', fg: 'text-white', hex: '#16142e', emoji: '✨' };
  return MOODS[Math.abs(id) % MOODS.length];
}

export const STATUS_STYLE: Record<string, string> = {
  Pending: 'bg-saffron-soft text-ink',
  Confirmed: 'bg-cobalt-soft text-ink',
  Shipped: 'bg-lilac-soft text-ink',
  Delivered: 'bg-mint-soft text-ink',
  Cancelled: 'bg-coral-soft text-ink',
  Completed: 'bg-mint-soft text-ink',
  Failed: 'bg-coral-soft text-ink',
  Refunded: 'bg-lilac-soft text-ink',
};

export const PAYMENT_LABEL: Record<string, string> = {
  CashOnDelivery: 'Cash on delivery',
  CreditCard: 'Card',
  DebitCard: 'Card',
  Wallet: 'Mobile wallet',
  PayPal: 'PayPal',
};

/** Local-date `yyyy-MM-ddTHH:mm` for <input type="datetime-local">. */
export function toLocalInput(iso?: string | null): string {
  const d = iso ? new Date(iso) : new Date();
  const p = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
}

export function debounce<T extends (...a: any[]) => void>(fn: T, ms: number) {
  let t: ReturnType<typeof setTimeout>;
  return (...a: Parameters<T>) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); };
}
