using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Address:BaseEntity<int>
    {
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;

        // Location URL (Google Maps link)
        public string? LocationUrl { get; set; }

        // Optional: owner user
        public Guid? UserId { get; set; }
        public User? User { get; set; }
    }
}
