using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Wishlist
{
    public class AddToWishlistDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a valid positive integer.")]
        public int ProductId { get; set; }
    }
}
