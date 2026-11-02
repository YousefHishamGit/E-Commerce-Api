namespace Application.DTOs.Wishlist
{
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public string? ProductPhoto { get; set; }
        public int StockQuantity { get; set; }
    }
}
