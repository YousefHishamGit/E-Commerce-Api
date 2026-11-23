using Domain.Enums;
using Domain.Entities;

namespace Application.Specifications
{
    public class OrdersWithDetailsSpec : BaseSpecification<Order>
    {
        // 1. All orders for a specific customer
        public OrdersWithDetailsSpec(Guid customerId)
            : base(o => o.CustomerId == customerId)
        {
            AddInclude(o => o.Customer);
            AddInclude(o => o.Coupon!);
            AddInclude(o => o.Payments);
            AddInclude("OrderItems.Product");
            AddOrderByDescending(o => o.CreatedAt);
        }

        // 2. Specific order for a specific customer
        public OrdersWithDetailsSpec(Guid orderId, Guid customerId)
            : base(o => o.Id == orderId && o.CustomerId == customerId)
        {
            AddInclude(o => o.Customer);
            AddInclude(o => o.Coupon!);
            AddInclude(o => o.Payments);
            AddInclude("OrderItems.Product");
        }

        // 3. Admin: Specific order by Id
        public OrdersWithDetailsSpec(Guid orderId, bool isAdmin)
            : base(o => o.Id == orderId)
        {
            AddInclude(o => o.Customer);
            AddInclude(o => o.Coupon!);
            AddInclude(o => o.Payments);
            AddInclude("OrderItems.Product");
        }

        // 4. Admin: All orders with paging
        public OrdersWithDetailsSpec(int pageIndex, int pageSize)
            : base()
        {
            AddInclude(o => o.Customer);
            AddInclude(o => o.Coupon!);
            AddInclude(o => o.Payments);
            AddInclude("OrderItems.Product");
            AddOrderByDescending(o => o.CreatedAt);
            ApplyPaging((pageIndex - 1) * pageSize, pageSize);
        }
    }

    /// <summary>Online-payment orders that were never paid within the allowed window.</summary>
    public class ExpiredUnpaidOrdersSpec : BaseSpecification<Order>
    {
        public ExpiredUnpaidOrdersSpec(DateTime cutoffUtc, int take)
            : base(o => o.Status == OrderStatus.Pending
                        && o.CreatedAt < cutoffUtc
                        && o.Payments.Any(p => p.Method != PaymentMethod.CashOnDelivery)
                        && !o.Payments.Any(p => p.Status == PaymentStatus.Completed))
        {
            AddInclude(o => o.Payments);
            AddInclude(o => o.OrderItems);
            AddOrderBy(o => o.CreatedAt);
            ApplyPaging(0, take);
        }
    }

    public class OrdersCountSpec : BaseSpecification<Order>
    {
        public OrdersCountSpec() : base()
        {
        }

        public OrdersCountSpec(Guid customerId) : base(o => o.CustomerId == customerId)
        {
        }
    }
}
