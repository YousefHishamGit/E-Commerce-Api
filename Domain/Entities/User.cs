using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User:BaseEntity<Guid>
    {
       
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public Gender Gender { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;

        public string Email { get; set; }
        public string HashPassword { get; set; }

        // Account security
        public bool EmailConfirmed { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }



        // Navigation
        public ICollection<Address> Addresses { get; set; } = new List<Address>();// may have many address 
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public Wishlist? Wishlist { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
