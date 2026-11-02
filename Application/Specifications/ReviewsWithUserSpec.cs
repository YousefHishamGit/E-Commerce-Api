using Domain.Entities;

namespace Application.Specifications
{
    public class ReviewsWithUserSpec : BaseSpecification<Review>
    {
        // Get all reviews for a specific product with user info, sorted by latest
        public ReviewsWithUserSpec(int productId)
            : base(r => r.ProductId == productId)
        {
            AddInclude(r => r.User);
            AddOrderByDescending(r => r.CreatedAt);
        }

        // Get reviews with pagination
        public ReviewsWithUserSpec(int productId, int pageIndex, int pageSize)
            : base(r => r.ProductId == productId)
        {
            AddInclude(r => r.User);
            AddOrderByDescending(r => r.CreatedAt);
            ApplyPaging((pageIndex - 1) * pageSize, pageSize);
        }

        // Get single review by its own ID with user info
        public ReviewsWithUserSpec(int reviewId, bool isReviewId)
            : base(r => r.Id == reviewId)
        {
            AddInclude(r => r.User);
        }

        // Get review by user and product (for checking existing review)
        public ReviewsWithUserSpec(Guid userId, int productId)
            : base(r => r.UserId == userId && r.ProductId == productId)
        {
            AddInclude(r => r.User);
        }
    }

    public class ReviewsCountForProductSpec : BaseSpecification<Review>
    {
        public ReviewsCountForProductSpec(int productId)
            : base(r => r.ProductId == productId)
        {
        }
    }
}
