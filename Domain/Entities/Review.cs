using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Review :BaseEntity<int>
    {
       

        // 1 - 5
        public int Rating { get; set; }
        public string? Comment { get; set; }
      

        // Optional: verified purchase
        public bool IsVerifiedPurchase { get; set; } = false;

        // FK
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

      
    }
}
