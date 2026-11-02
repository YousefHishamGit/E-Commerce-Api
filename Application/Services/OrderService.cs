using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.DTOs.Common;
using Application.DTOs.Orders;
using Application.Exceptions;
using Application.Specifications;
using AutoMapper;
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

        public OrderService(
            IUnitOfWork uow,
            IBasketRepository basketRepo,
            ICouponRepository couponRepo,
            ICurrentUser currentUser,
            IMapper mapper)
        {
            _uow = uow;
            _basketRepo = basketRepo;
            _couponRepo = couponRepo;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;

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
                coupon = await _couponRepo.GetByCodeAsync(couponCodeToApply);
                var now = DateTime.UtcNow;

                if (coupon is not null && coupon.IsActive && coupon.StartDate <= now && coupon.EndDate >= now)
                {
                    if (!coupon.MaxUsage.HasValue || coupon.UsedCount < coupon.MaxUsage.Value)
                    {
                        var itemsSubtotal = orderItems.Sum(oi => oi.SubTotal);
                        if (!coupon.MinOrderAmount.HasValue || itemsSubtotal >= coupon.MinOrderAmount.Value)
                        {
                            couponDiscountAmount = coupon.Type == CouponType.Percentage
                                ? Math.Round((itemsSubtotal * coupon.Value) / 100m, 2)
                                : Math.Min(coupon.Value, itemsSubtotal);

                            coupon.UsedCount++;
                            _couponRepo.Update(coupon);
                        }
                        else
                        {
                            // Coupon min-order not met — silently skip; don't block checkout
                            coupon = null;
                        }
                    }
                    else
                    {
                        coupon = null; // exhausted
                    }
                }
                else
                {
                    coupon = null; // expired / inactive
                }
            }

            // 6. Calculate totals
            var subtotal = orderItems.Sum(oi => oi.SubTotal);
            var shipping = basket.ShippingCost ?? 0m;
            var taxes = basket.TaxesCost ?? 0m;
            var totalPrice = Math.Max(0m, subtotal + shipping + taxes - couponDiscountAmount);

            // 7. Parse payment method
            if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var paymentMethod))
                paymentMethod = PaymentMethod.CashOnDelivery;

            // 8. Persist Order with address snapshot
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                CustomerId = userId,
                Status = OrderStatus.Pending,
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
            return _mapper.Map<OrderDto>(createdOrders.First());
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
            if (orderStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                await RestoreOrderInventoryAsync(order);
                await RevertCouponUsageAsync(order);
            }

            order.Status = orderStatus;
            _uow.GetRepository<Order, Guid>().Update(order);
            await _uow.SaveChangesAsync();

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

            var refreshedOrders = await _uow.GetRepository<Order, Guid>().GetAllAsync(spec);
            return _mapper.Map<OrderDto>(refreshedOrders.First());
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
