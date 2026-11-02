namespace Application.DTOs.Reviews
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = null!;
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
