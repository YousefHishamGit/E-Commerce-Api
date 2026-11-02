using Domain.Entities;
using System;

namespace Application.Specifications
{
    public class ActiveOffersSpec : BaseSpecification<Offer>
    {
        // Get all active offers within current valid date range
        public ActiveOffersSpec()
            : base(o => o.IsActive && o.StartDate <= DateTime.UtcNow && o.EndDate >= DateTime.UtcNow)
        {
            AddInclude(o => o.Product!);
            AddInclude(o => o.Category!);
            AddOrderByDescending(o => o.DiscountPercentage);
        }

        // Get active offer for a specific product
        public ActiveOffersSpec(int productId)
            : base(o => o.IsActive && o.ProductId == productId && o.StartDate <= DateTime.UtcNow && o.EndDate >= DateTime.UtcNow)
        {
            AddInclude(o => o.Product!);
        }
    }

    public class OffersWithDetailsSpec : BaseSpecification<Offer>
    {
        // Get all offers (for admin management) with Product and Category
        public OffersWithDetailsSpec()
            : base()
        {
            AddInclude(o => o.Product!);
            AddInclude(o => o.Category!);
            AddOrderByDescending(o => o.StartDate);
        }

        // Get single offer by ID with Product and Category
        public OffersWithDetailsSpec(int offerId)
            : base(o => o.Id == offerId)
        {
            AddInclude(o => o.Product!);
            AddInclude(o => o.Category!);
        }
    }
}
