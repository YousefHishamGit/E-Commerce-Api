using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Offers
{
    public class UpdateOfferDto
    {
        [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string? Title { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(1, 100, ErrorMessage = "DiscountPercentage must be between 1 and 100.")]
        public decimal? DiscountPercentage { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsActive { get; set; }
    }
}
