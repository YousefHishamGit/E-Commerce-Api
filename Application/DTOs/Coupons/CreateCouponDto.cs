using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Coupons
{
    public class CreateCouponDto
    {
        [Required(ErrorMessage = "Coupon code is required.")]
        [MaxLength(50, ErrorMessage = "Coupon code cannot exceed 50 characters.")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Coupon type is required ('Percentage' or 'FixedAmount').")]
        public string Type { get; set; } = null!;   // "Percentage" or "FixedAmount"

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Coupon value must be greater than 0.")]
        public decimal Value { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MinOrderAmount cannot be negative.")]
        public decimal? MinOrderAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxUsage must be at least 1.")]
        public int? MaxUsage { get; set; }
    }
}
