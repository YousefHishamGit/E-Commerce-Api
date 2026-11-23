using Application.Common;
using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.DTOs.Common;
using Application.DTOs.Orders;
using Application.Exceptions;
using Application.Specifications;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IBasketRepository _basketRepo;
        private readonly ICouponRepository _couponRepo;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;
        private readonly ShopSettings _shop;
        private readonly IEmailSender _email;
        private readonly IPaymentService _payments;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork uow,
            IBasketRepository basketRepo,
            ICouponRepository couponRepo,
            ICurrentUser currentUser,
            IMapper mapper,
            ShopSettings shop,
            IEmailSender email,
            IPaymentService payments,
            ILogger<OrderService> logger)
        {
            _shop = shop;
            _email = email;
            _payments = payments;
            _logger = logger;
            _uow = uow;
            _basketRepo = basketRepo;
            _couponRepo = couponRepo;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;

            // 0. Only customers with a confirmed email can place orders
            var customer = await _uow.GetRepository<User, Guid>().GetByIdAsync(userId)
                           ?? throw new NotFoundException("User was not found.");
            if (!customer.EmailConfirmed)
                throw new ForbiddenException("Please confirm your email address before placing an order.");

            // 1. Validate delivery address belongs to this user
            var addressSpec = new AddressesByUserSpec(dto.DeliveryAddressId, userId);
            var address = await _uow.GetRepository<Address, int>().GetByIdAsync(dto.DeliveryAddressId, addressSpec);
            if (address is null)
                throw new NotFoundException($"Delivery address with ID {dto.DeliveryAddressId} was not found or does not belong to you.");

            // 2. Load basket from Redis
            var basketKey = string.IsNullOrWhiteSpace(dto.BasketId)
                ? CacheKeys.Basket(userId)
                : dto.BasketId;

            var basket = await _basketRepo.GetBasketAsync(basketKey);
            if (basket is null || !basket.Items.Any())
                throw new BadRequestException("Your basket is empty. Please add items to your basket before checking out.");

            // 3. Fetch ALL active offers once (avoid N+1 per product)
            var offerRepo = _uow.GetRepository<Offer, int>();
            var activeOffers = (await offerRepo.GetAllAsync(new ActiveOffersSpec())).ToList();

            // 4. Build order items — re-validate stock & re-compute price from DB (never trust Redis cache)
            var productRepo = _uow.GetRepository<Product, int>();
            var orderItems = new List<OrderItem>();

            foreach (var item in basket.Items)
            {
                var product = await productRepo.GetByIdAsync(item.Id);
                if (product is null)
                    throw new NotFoundException($"Product '{item.ProductName}' (ID: {item.Id}) was not found.");

                if (product.StockQuantity < item.Quantity)
                    throw new BadRequestException($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, Requested: {item.Quantity}.");

                // Re-calculate live price from DB — ignore basket-cached price (may be stale)
                var applicableOffer = activeOffers
                    .Where(o =>
                        (o.ProductId.HasValue && o.ProductId.Value == product.Id) ||
                        (o.CategoryId.HasValue && o.CategoryId.Value == product.CategoryId))
                    .OrderByDescending(o => o.DiscountPercentage)
                    .FirstOrDefault();

                var liveUnitPrice = applicableOffer is not null
                    ? product.Price - Math.Round((product.Price * applicableOffer.DiscountPercentage) / 100m, 2)
                    : product.Price;

                // Deduct stock
                product.StockQuantity -= item.Quantity;
                productRepo.Update(product);

                orderItems.Add(new OrderItem
                {
                    ProductId = item.Id,
                    Quantity = item.Quantity,
                    UnitPrice = liveUnitPrice  // always fresh from DB
                });
            }

            // 5. Validate & apply coupon
            var couponCodeToApply = !string.IsNullOrWhiteSpace(dto.CouponCode)
                ? dto.CouponCode.Trim()
                : basket.CouponCode;

            Coupon? coupon = null;
            decimal couponDiscountAmount = 0m;

            if (!string.IsNullOrWhiteSpace(couponCodeToApply))
            {
                coupon = await _couponRepo.GetByCodeAsync(couponCodeToApply.Trim().ToUpperInvariant());
                var now = DateTime.UtcNow;

                if (coupon is null)
                    throw new BadRequestException($"Coupon '{couponCodeToApply}' was not found.");
                if (!coupon.IsActive || coupon.StartDate > now || coupon.EndDate < now)
                    throw new BadRequestException("This coupon is inactive or expired.");
                if (coupon.MaxUsage.HasValue && coupon.UsedCount >= coupon.MaxUsage.Value)
                    throw new BadRequestException("This coupon has reached its maximum usage limit.");

                var itemsSubtotal = orderItems.Sum(oi => oi.SubTotal);
                if (coupon.MinOrderAmount.HasValue && itemsSubtotal < coupon.MinOrderAmount.Value)
                    throw new BadRequestException($"Minimum order amount of {coupon.MinOrderAmount.Value:0.##} is required to apply this coupon.");

                couponDiscountAmount = coupon.Type == CouponType.Percentage
                    ? Math.Round((itemsSubtotal * coupon.Value) / 100m, 2)
                    : Math.Min(coupon.Value, itemsSubtotal);

                coupon.UsedCount++;
                _couponRepo.Update(coupon);
            }

            // 6. Calculate totals
            //    Shipping and taxes are computed here from server-side rules - never from the basket/client.
            var totals = PricingCalculator.Calculate(orderItems.Sum(oi => oi.SubTotal), couponDiscountAmount, _shop);
            var totalPrice = totals.Total;

            // 7. Parse payment method (reject unknown values instead of silently defaulting)
            if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var paymentMethod)
                || !Enum.IsDefined(paymentMethod))
                throw new BadRequestException($"Invalid payment method '{dto.PaymentMethod}'. Allowed: CashOnDelivery, CreditCard, DebitCard, Wallet.");
            if (paymentMethod == PaymentMethod.PayPal)
                throw new BadRequestException("PayPal is not supported. Use CashOnDelivery, CreditCard, DebitCard or Wallet.");

            // 8. Persist Order with address snapshot
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                CustomerId = userId,
                Status = OrderStatus.Pending,
                SubTotal = totals.SubTotal,
                DiscountAmount = totals.Discount,
                ShippingCost = totals.Shipping,
                TaxAmount = totals.Tax,
                TotalPrice = totalPrice,
                CouponId = coupon?.Id,
                OrderItems = orderItems,
                CreatedAt = DateTime.UtcNow,

                // Snapshot delivery address — immune to future address edits
                ShippingStreet = address.Street,
                ShippingCity = address.City,
                ShippingLocationUrl = address.LocationUrl
            };

            var payment = new Payment
            {
                OrderId = orderId,
                Method = paymentMethod,
                Status = PaymentStatus.Pending,
                Amount = totalPrice,
                CreatedAt = DateTime.UtcNow
            };
            order.Payments.Add(payment);

            await _uow.GetRepository<Order, Guid>().AddAsync(order);
            await _uow.SaveChangesAsync();

            // 9. Clear basket from Redis
            await _basketRepo.DeleteBasketAsync(basketKey);

            // 10. Return full order details
            var spec = new OrdersWithDetailsSpec(order.Id, userId);
            var createdOrders = await _uow.GetRepository<Order, Guid>().GetAllAsync(spec);
            var result = _mapper.Map<OrderDto>(createdOrders.First());

            // 11. Confirmation email (best effort: a mail problem must never fail a placed order)
            try
            {
                var (subject, html) = EmailTemplates.OrderConfirmation(customer.FirstName, result);
                await _email.SendAsync(customer.Email, subject, html, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email for order {OrderId}", result.Id);
            }

            return result;
        }

        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            var spec = new OrdersWithDetailsSpec(userId);
            var orders = await _uow.GetRepository<Order, Guid>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            var isAdmin = _currentUser.Role == UserRole.Admin;
            var spec = isAdmin
                ? new OrdersWithDetailsSpec(orderId, true)
                : new OrdersWithDetailsSpec(orderId, _currentUser.UserId);

            var orders = await _uow.GetRepository<Order, Guid>().GetAllAsync(spec);
            var order = orders.FirstOrDefault();

            if (order is null)
                throw new NotFoundException($"Order with ID {orderId} was not found.");

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<PaginatedResponseDto<OrderDto>> GetAllOrdersAsync(int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var countSpec = new OrdersCountSpec();
            var totalCount = await _uow.GetRepository<Order, Guid>().CountAsync(countSpec);

            var spec = new OrdersWithDetailsSpec(pageIndex, pageSize);
            var orders = await _uow.GetRepository<Order, Guid>().GetAllAsync(spec);

            return new PaginatedResponseDto<OrderDto>
            {
                Data = _mapper.Map<IEnumerable<OrderDto>>(orders),
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<OrderStatus>(newStatus, true, out var orderStatus))
                throw new BadRequestException($"Invalid order status '{newStatus}'. Allowed: Pending, Confirmed, Shipped, Delivered, Cancelled.");

            var spec = new OrdersWithDetailsSpec(orderId, true);
            var orders = await _uow.GetRepository<Order, Guid>().GetAllAsync(spec);
            var order = orders.FirstOrDefault();

            if (order is null)
                throw new NotFoundException($"Order with ID {orderId} was not found.");

            // Restore inventory & revert coupon when cancelling
            var cancelling = orderStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled;
            if (cancelling)
            {
                await RestoreOrderInventoryAsync(order);
                await RevertCouponUsageAsync(order);
            }

            order.Status = orderStatus;
            _uow.GetRepository<Order, Guid>().Update(order);
            await _uow.SaveChangesAsync();

            if (cancelling) await RefundPaidOnlinePaymentsAsync(order, cancellationToken);

            var refreshedOrders = await _uow.GetRepository<Order, Guid>().GetAllAsync(spec);
            return _mapper.Map<OrderDto>(refreshedOrders.First());
        }

        public async Task<OrderDto> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            var isAdmin = _currentUser.Role == UserRole.Admin;
            var spec = isAdmin
                ? new OrdersWithDetailsSpec(orderId, true)
                : new OrdersWithDetailsSpec(orderId, _currentUser.UserId);

            var orders = await _uow.GetRepository<Order, Guid>().GetAllAsync(spec);
            var order = orders.FirstOrDefault();

            if (order is null)
                throw new NotFoundException($"Order with ID {orderId} was not found.");

            if (order.Status == OrderStatus.Delivered)
                throw new BadRequestException("Delivered orders cannot be cancelled.");

            if (order.Status == OrderStatus.Cancelled)
                throw new BadRequestException("Order is already cancelled.");

            if (!isAdmin && order.Status == OrderStatus.Shipped)
                throw new BadRequestException("Order has already been shipped and cannot be cancelled by customer.");

            // Restore stock and revert coupon usage
            await RestoreOrderInventoryAsync(order);
            await RevertCouponUsageAsync(order);

            order.Status = OrderStatus.Cancelled;
            _uow.GetRepository<Order, Guid>().Update(order);
            await _uow.SaveChangesAsync();

            await RefundPaidOnlinePaymentsAsync(order, cancellationToken);

            var refreshedOrders = await _uow.GetRepository<Order, Guid>().GetAllAsync(spec);
            return _mapper.Map<OrderDto>(refreshedOrders.First());
        }

        public async Task<int> CancelExpiredUnpaidOrdersAsync(int olderThanMinutes, CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-olderThanMinutes);
            var expired = (await _uow.GetRepository<Order, Guid>()
                .GetAllAsync(new ExpiredUnpaidOrdersSpec(cutoff, take: 50))).ToList();

            var repo = _uow.GetRepository<Order, Guid>();
            foreach (var order in expired)
            {
                await RestoreOrderInventoryAsync(order);
                await RevertCouponUsageAsync(order);

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                repo.Update(order);
                await _uow.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} cancelled: online payment not completed within {Minutes} minutes.", order.Id, olderThanMinutes);
            }
            return expired.Count;
        }

        /// <summary>When a paid online order is cancelled the money must go back to the customer.</summary>
        private async Task RefundPaidOnlinePaymentsAsync(Order order, CancellationToken ct)
        {
            foreach (var payment in order.Payments.Where(p => p.Status == PaymentStatus.Completed && p.Method.IsOnline()))
                await _payments.TryRefundAsync(payment, ct);
        }

        private async Task RestoreOrderInventoryAsync(Order order)
        {
            var productRepo = _uow.GetRepository<Product, int>();

            // Group items by product to handle duplicates safely
            var itemsToRestore = order.OrderItems
                .GroupBy(item => item.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
                .ToList();

            foreach (var item in itemsToRestore)
            {
                var product = await productRepo.GetByIdAsync(item.ProductId);
                if (product is null) continue; // product deleted — skip silently

                product.StockQuantity = checked(product.StockQuantity + item.Quantity);
                productRepo.Update(product);
            }
        }

        private async Task RevertCouponUsageAsync(Order order)
        {
            if (!order.CouponId.HasValue) return;

            var coupon = await _couponRepo.GetByIdAsync(order.CouponId.Value);
            if (coupon is not null && coupon.UsedCount > 0)
            {
                coupon.UsedCount--;
                _couponRepo.Update(coupon);
            }
        }
    }
}
