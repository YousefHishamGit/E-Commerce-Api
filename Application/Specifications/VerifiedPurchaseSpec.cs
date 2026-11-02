using Domain.Entities;
using System;

namespace Application.Specifications
{
    /// <summary>
    /// Checks whether a user has purchased a specific product (verified purchase).
    /// Used by ReviewService to flag reviews as verified.
    /// </summary>
    public class VerifiedPurchaseSpec : BaseSpecification<OrderItem>
    {
        public VerifiedPurchaseSpec(Guid userId, int productId)
            : base(oi => oi.ProductId == productId && oi.Order.CustomerId == userId)
        {
            AddInclude(oi => oi.Order);
        }
    }
}

