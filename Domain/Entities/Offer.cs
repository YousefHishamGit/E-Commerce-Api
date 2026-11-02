using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Offer:BaseEntity<int>
    {
       
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

       
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
