using Domain.Entities;

namespace Application.Specifications
{
    public class WishlistWithItemsSpec : BaseSpecification<Wishlist>
    {
        // Get user's wishlist including wishlist items and their product details
        public WishlistWithItemsSpec(Guid userId)
            : base(w => w.UserId == userId)
        {
            AddInclude(w => w.Items);
            AddInclude("Items.Product");
        }
    }
}

