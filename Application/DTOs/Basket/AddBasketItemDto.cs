using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Basket
{
    public class AddBasketItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;
    }
}

