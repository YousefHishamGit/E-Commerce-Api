using Application.DTOs.Common;
using Application.DTOs.Reviews;

namespace Application.Contracts.Services
{
    public interface IReviewService
    {
 
        Task<PaginatedResponseDto<ReviewDto>> GetProductReviewsAsync(int productId, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<ReviewDto> GetReviewByIdAsync(int reviewId, CancellationToken cancellationToken = default);

        Task<ReviewDto> AddReviewAsync(CreateReviewDto dto, CancellationToken cancellationToken = default);

        Task<ReviewDto> UpdateReviewAsync(int reviewId, UpdateReviewDto dto, CancellationToken cancellationToken = default);

        Task DeleteReviewAsync(int reviewId, CancellationToken cancellationToken = default);
    }
}

