namespace Application.DTOs.Coupons
{
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Type { get; set; } = null!;      // "Percentage" or "FixedAmount"
        public decimal Value { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUsage { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }
    }
}
