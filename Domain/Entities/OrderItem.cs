using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderItem:BaseEntity<int>
    {
        
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }   // snapshot of price at order time

        public decimal SubTotal => Quantity * UnitPrice;

        // FK
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
