using System;

namespace Application.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal? PriceAfterDiscount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? OfferTitle { get; set; }
        public bool HasOffer => DiscountPercentage.HasValue && DiscountPercentage.Value > 0;
        /// <summary>Main image (relative URL). Same as the first item of Images.</summary>
        public string? Photo { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
        public string? Description { get; set; }
        public int StockQuantity { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
