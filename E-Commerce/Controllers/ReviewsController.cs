using Application.Contracts.Services;
using Application.DTOs.Common;
using Application.DTOs.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace E_Commerce.Controllers
{
    public class ReviewsController : BaseApiController
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Get paginated reviews for a specific product.
        /// </summary>
        [HttpGet("product/{productId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<PaginatedResponseDto<ReviewDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<PaginatedResponseDto<ReviewDto>>>> GetProductReviews(
            [FromRoute] int productId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _reviewService.GetProductReviewsAsync(productId, pageIndex, pageSize, cancellationToken);
            return Ok(ApiResponseDto<PaginatedResponseDto<ReviewDto>>.Ok(result));
        }

        /// <summary>
        /// Get a single review by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<ReviewDto>>> GetReviewById(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var review = await _reviewService.GetReviewByIdAsync(id, cancellationToken);
            return Ok(ApiResponseDto<ReviewDto>.Ok(review));
        }

        /// <summary>
        /// Add a new review to a product (Authenticated users only).
        /// </summary>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<ReviewDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<ReviewDto>>> AddReview(
            [FromBody] CreateReviewDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _reviewService.AddReviewAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetReviewById),
                new { id = created.Id },
                ApiResponseDto<ReviewDto>.Created(created, "Review added successfully."));
        }

        /// <summary>
        /// Update an existing review (Review owner only).
        /// </summary>
        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<ReviewDto>>> UpdateReview(
            [FromRoute] int id,
            [FromBody] UpdateReviewDto dto,
            CancellationToken cancellationToken = default)
        {
            var updated = await _reviewService.UpdateReviewAsync(id, dto, cancellationToken);
            return Ok(ApiResponseDto<ReviewDto>.Ok(updated, "Review updated successfully."));
        }

        /// <summary>
        /// Delete a review (Review owner or Admin only).
        /// </summary>
        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<string>>> DeleteReview(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            await _reviewService.DeleteReviewAsync(id, cancellationToken);
            return Ok(ApiResponseDto<string>.Ok($"Review with ID {id} has been deleted successfully."));
        }
    }
}

