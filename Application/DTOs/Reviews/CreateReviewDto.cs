using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reviews
{
    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a valid positive number.")]
        public int ProductId { get; set; }
    }
}
