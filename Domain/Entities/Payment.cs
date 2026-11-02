using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Payment:BaseEntity<int>
    {
       

        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public decimal Amount { get; set; }

        // Transaction reference from gateway (Stripe / PayPal / etc.)
        public string? TransactionId { get; set; }

        // Last 4 digits / masked card number (never store full card!)
        public string? MaskedCardNumber { get; set; }

      
        public DateTime? PaidAt { get; set; }

        // FK
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
