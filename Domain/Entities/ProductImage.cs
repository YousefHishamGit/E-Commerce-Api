namespace Domain.Entities
{
    public class ProductImage : BaseEntity<int>
    {
        public const int MaxPerProduct = 4;

        /// <summary>Relative public URL, e.g. /uploads/products/abc.jpg</summary>
        public string Url { get; set; } = null!;

        /// <summary>Slot 0..3. The lowest slot is the product's main image.</summary>
        public int SortOrder { get; set; }

        // FK
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
