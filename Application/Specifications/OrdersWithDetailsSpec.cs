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
