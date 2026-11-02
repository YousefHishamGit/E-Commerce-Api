using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Coupons
{
    public class ApplyCouponDto
    {
        [Required(ErrorMessage = "Coupon code is required.")]
        public string Code { get; set; } = null!;

        [Range(0, double.MaxValue, ErrorMessage = "OrderAmount cannot be negative.")]
        public decimal OrderAmount { get; set; }
    }

    public class ApplyCouponToBasketDto
    {
        [Required(ErrorMessage = "Coupon code is required.")]
        public string Code { get; set; } = null!;
    }
}
