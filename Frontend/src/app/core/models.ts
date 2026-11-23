/** Mirrors the DTOs of the ASP.NET API (camelCase JSON). */

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  statusCode: number;
  errors?: string[];
}

export interface Paginated<T> {
  data: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export type Role = 'Customer' | 'Admin';

export interface AuthResponse {
  userId: string;
  email: string;
  fullName: string;
  role: Role;
  emailConfirmed: boolean;
  token: string;
  tokenExpiry: string;
  refreshToken: string;
  refreshTokenExpiry: string;
}

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phone: string;
  email: string;
  gender: string;
  role: Role;
  emailConfirmed: boolean;
  createdAt: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
  password: string;
  confirmPassword: string;
  gender: 'Male' | 'Female';
}

export interface ProductImage { id: number; url: string; isMain: boolean; }

export interface Product {
  id: number;
  name: string;
  price: number;
  priceAfterDiscount?: number | null;
  discountPercentage?: number | null;
  offerTitle?: string | null;
  hasOffer: boolean;
  photo?: string | null;
  images: ProductImage[];
  description?: string | null;
  stockQuantity: number;
  averageRating: number;
  reviewsCount: number;
  categoryId: number;
  categoryName: string;
  createdAt: string;
}

export interface ProductQuery {
  pageIndex?: number;
  pageSize?: number;
  categoryId?: number | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  sort?: string;
  search?: string;
}

export interface Category { id: number; name: string; description?: string | null; productCount: number; }

export interface BasketItem { id: number; productName: string; pictureUrl: string; price: number; quantity: number; }

export interface Basket {
  id: string;
  items: BasketItem[];
  couponCode?: string | null;
  couponDiscount?: number | null;
  offerDiscount?: number | null;
  shippingCost?: number | null;
  taxesCost?: number | null;
  totalPrice?: number | null;
}

export interface WishlistItem {
  id: number; productId: number; productName: string; productPrice: number; productPhoto?: string | null; stockQuantity: number;
}
export interface Wishlist { id: number; userId: string; items: WishlistItem[]; totalItems: number; }

export interface Address { id: number; city: string; street: string; locationUrl?: string | null; }
export type AddressInput = Omit<Address, 'id'>;

export type OrderStatus = 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled';
export type PaymentMethod = 'CashOnDelivery' | 'CreditCard' | 'DebitCard' | 'PayPal' | 'Wallet';

export interface Payment {
  id: number; method: string; status: 'Pending' | 'Completed' | 'Failed' | 'Refunded'; amount: number;
  transactionId?: string | null; maskedCardNumber?: string | null; paidAt?: string | null; orderId: string;
}

export interface OrderItem {
  id: number; productId: number; productName: string; productPhoto?: string | null; unitPrice: number; quantity: number; subTotal: number;
}

export interface Order {
  id: string;
  status: OrderStatus;
  subTotal: number; discountAmount: number; shippingCost: number; taxAmount: number; totalPrice: number;
  couponCode?: string | null;
  customerId: string; customerName: string;
  shippingCity: string; shippingStreet: string; shippingLocationUrl?: string | null;
  items: OrderItem[]; payments: Payment[];
  createdAt: string;
}

export interface PayOrderResponse { paymentId: number; orderId: string; amount: number; currency: string; checkoutUrl: string; }

export interface Review {
  id: number; rating: number; comment?: string | null; isVerifiedPurchase: boolean;
  userId: string; userFullName: string; productId: number; createdAt: string;
}

export interface CouponValidation { isValid: boolean; message?: string | null; discountAmount: number; finalAmount: number; }

export interface Coupon {
  id: number; code: string; type: 'Percentage' | 'FixedAmount'; value: number; minOrderAmount?: number | null;
  startDate: string; endDate: string; maxUsage?: number | null; usedCount: number; isActive: boolean;
}

export interface Offer {
  id: number; title: string; description?: string | null; discountPercentage: number;
  startDate: string; endDate: string; isActive: boolean;
  productId?: number | null; productName?: string | null; categoryId?: number | null; categoryName?: string | null;
}

export interface DashboardSummary {
  totalCustomers: number; totalProducts: number; totalOrders: number; pendingOrders: number;
  lowStockProducts: number; outOfStockProducts: number; totalRevenue: number; averageOrderValue: number;
  todayOrders: number; todayRevenue: number; newCustomersLast30Days: number;
}
export interface SalesPoint { date: string; ordersCount: number; revenue: number; }
export interface TopProduct { productId: number; name: string; photo?: string | null; quantitySold: number; revenue: number; }
export interface LowStockProduct { productId: number; name: string; categoryName: string; stockQuantity: number; }
export interface OrderStatusCount { status: string; count: number; }
export interface RecentOrder { orderId: string; customerName: string; status: string; totalPrice: number; itemsCount: number; createdAt: string; }
