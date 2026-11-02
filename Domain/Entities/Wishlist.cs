using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Wishlist : BaseEntity<int>
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
    }
}
