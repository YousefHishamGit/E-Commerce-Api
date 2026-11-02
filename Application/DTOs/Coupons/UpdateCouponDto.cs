using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Coupons
{
    public class UpdateCouponDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Coupon value must be greater than 0.")]
        public decimal? Value { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MinOrderAmount cannot be negative.")]
        public decimal? MinOrderAmount { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxUsage must be at least 1.")]
        public int? MaxUsage { get; set; }

        public bool? IsActive { get; set; }
    }
}
