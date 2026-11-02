using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.DTOs.Common;
using Application.DTOs.Reviews;
using Application.Exceptions;
using Application.Specifications;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public ReviewService(
            IUnitOfWork uow,
            IMapper mapper,
            ICurrentUser currentUser)
        {
            _uow = uow;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<PaginatedResponseDto<ReviewDto>> GetProductReviewsAsync(
            int productId,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var product = await _uow.GetRepository<Product, int>().GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException($"Product with ID {productId} was not found.");

            var countSpec = new ReviewsCountForProductSpec(productId);
            var totalCount = await _uow.GetRepository<Review, int>().CountAsync(countSpec);

            var spec = new ReviewsWithUserSpec(productId, pageIndex, pageSize);
            var reviews = await _uow.GetRepository<Review, int>().GetAllAsync(spec);

            var data = _mapper.Map<System.Collections.Generic.IEnumerable<ReviewDto>>(reviews);

            return new PaginatedResponseDto<ReviewDto>
            {
                Data = data,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ReviewDto> GetReviewByIdAsync(int reviewId, CancellationToken cancellationToken = default)
        {
            var spec = new ReviewsWithUserSpec(reviewId, true);
            var reviews = await _uow.GetRepository<Review, int>().GetAllAsync(spec);
            var review = reviews.FirstOrDefault();

            if (review is null)
                throw new NotFoundException($"Review with ID {reviewId} was not found.");

            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<ReviewDto> AddReviewAsync(CreateReviewDto dto, CancellationToken cancellationToken = default)
        {
            var product = await _uow.GetRepository<Product, int>().GetByIdAsync(dto.ProductId);
            if (product is null)
                throw new NotFoundException($"Product with ID {dto.ProductId} was not found.");

            var userId = _currentUser.UserId;

            // Prevent duplicate review for the same product by the same user
            var existingSpec = new ReviewsWithUserSpec(userId, dto.ProductId);
            var existingReviews = await _uow.GetRepository<Review, int>().GetAllAsync(existingSpec);
            if (existingReviews.Any())
                throw new ConflictException("You have already reviewed this product.");

            var review = _mapper.Map<Review>(dto);
            review.UserId = userId;
            review.CreatedAt = DateTime.UtcNow;

            // Check if user has purchased this product
            review.IsVerifiedPurchase = await CheckVerifiedPurchaseAsync(userId, dto.ProductId);

            await _uow.GetRepository<Review, int>().AddAsync(review);
            await _uow.SaveChangesAsync();

            // Recalculate product average rating and reviews count
            await UpdateProductRatingAsync(dto.ProductId);

            // Return created review with user details
            var spec = new ReviewsWithUserSpec(review.Id, true);
            var createdReviews = await _uow.GetRepository<Review, int>().GetAllAsync(spec);
            return _mapper.Map<ReviewDto>(createdReviews.First());
        }

        public async Task<ReviewDto> UpdateReviewAsync(int reviewId, UpdateReviewDto dto, CancellationToken cancellationToken = default)
        {
            var review = await _uow.GetRepository<Review, int>().GetByIdAsync(reviewId);
            if (review is null)
                throw new NotFoundException($"Review with ID {reviewId} was not found.");

            if (review.UserId != _currentUser.UserId)
                throw new ForbiddenException("You are not allowed to update this review.");

            _mapper.Map(dto, review);
            _uow.GetRepository<Review, int>().Update(review);
            await _uow.SaveChangesAsync();

            // Recalculate rating if changed
            if (dto.Rating.HasValue)
            {
                await UpdateProductRatingAsync(review.ProductId);
            }

            var spec = new ReviewsWithUserSpec(review.Id, true);
            var updatedReviews = await _uow.GetRepository<Review, int>().GetAllAsync(spec);
            return _mapper.Map<ReviewDto>(updatedReviews.First());
        }

        public async Task DeleteReviewAsync(int reviewId, CancellationToken cancellationToken = default)
        {
            var review = await _uow.GetRepository<Review, int>().GetByIdAsync(reviewId);
            if (review is null)
                throw new NotFoundException($"Review with ID {reviewId} was not found.");

            if (review.UserId != _currentUser.UserId && _currentUser.Role != UserRole.Admin)
                throw new ForbiddenException("You are not allowed to delete this review.");

            var productId = review.ProductId;

            _uow.GetRepository<Review, int>().Delete(review);
            await _uow.SaveChangesAsync();

            // Recalculate product rating
            await UpdateProductRatingAsync(productId);
        }


        private async Task UpdateProductRatingAsync(int productId)
        {
            var product = await _uow.GetRepository<Product, int>().GetByIdAsync(productId);
            if (product is null) return;

            var allSpec = new ReviewsWithUserSpec(productId);
            var allReviews = (await _uow.GetRepository<Review, int>().GetAllAsync(allSpec)).ToList();

            product.ReviewsCount = allReviews.Count;
            product.AverageRating = allReviews.Any()
                ? Math.Round(allReviews.Average(r => r.Rating), 1)
                : 0;

            _uow.GetRepository<Product, int>().Update(product);
            await _uow.SaveChangesAsync();
        }

        private async Task<bool> CheckVerifiedPurchaseAsync(Guid userId, int productId)
        {
            var spec = new VerifiedPurchaseSpec(userId, productId);
            return await _uow.GetRepository<OrderItem, int>().AnyAsync(spec);
        }
    }
}

