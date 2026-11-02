namespace Application.DTOs.Wishlist
{
    public class WishlistDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public List<WishlistItemDto> Items { get; set; } = new();
        public int TotalItems => Items.Count;
    }
}
