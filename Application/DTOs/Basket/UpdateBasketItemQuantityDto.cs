using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Basket
{
    public class UpdateBasketItemQuantityDto
    {
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }
    }
}

