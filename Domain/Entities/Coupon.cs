using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Coupon:BaseEntity<int>
    {
      
        public string Code { get; set; } = null!;
        public CouponType Type { get; set; }
        public decimal Value { get; set; }          // % or fixed amount
        public decimal? MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUsage { get; set; }
        public int UsedCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Optimistic concurrency token (prevents exceeding MaxUsage)
        public byte[] RowVersion { get; set; } = null!;

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
