using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Offers
{
    public class CreateOfferDto
    {
        [Required]
        [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "DiscountPercentage must be between 1 and 100.")]
        public decimal DiscountPercentage { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int? ProductId { get; set; }

        public int? CategoryId { get; set; }
    }
}
