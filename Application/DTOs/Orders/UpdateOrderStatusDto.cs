using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Orders
{
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Status is required (e.g. Confirmed, Shipped, Delivered, Cancelled).")]
        public string Status { get; set; } = null!;
    }
}
