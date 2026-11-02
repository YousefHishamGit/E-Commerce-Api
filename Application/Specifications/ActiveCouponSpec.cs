using Domain.Entities;

namespace Application.Specifications
{
    public class ActiveCouponSpec : BaseSpecification<Coupon>
    {
        // Find valid active coupon by code within date range
        public ActiveCouponSpec(string code)
            : base(c => c.Code.ToLower() == code.Trim().ToLower()
                     && c.IsActive
                     && c.StartDate <= DateTime.UtcNow
                     && c.EndDate >= DateTime.UtcNow
                     && (!c.MaxUsage.HasValue || c.UsedCount < c.MaxUsage.Value))
        {
        }
    }
}

