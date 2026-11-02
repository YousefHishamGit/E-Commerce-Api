namespace Application.DTOs.Coupons
{
    public class CouponValidationResultDto
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
