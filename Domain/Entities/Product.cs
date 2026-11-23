using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Product:BaseEntity<int>
    {
       
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Photo { get; set; }
        public string? Description { get; set; }
        public int StockQuantity { get; set; }
        public double AverageRating { get; set; } = 0;
        public int ReviewsCount { get; set; } = 0;

        // Optimistic concurrency token (prevents overselling stock)
        public byte[] RowVersion { get; set; } = null!;

        // FK
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
