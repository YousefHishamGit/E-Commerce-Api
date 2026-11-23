using Application.Contracts.Repository;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context) => _context = context;

        public Task<Payment?> GetByGatewayReferenceAsync(string reference)
            => _context.Payments.Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.GatewayReference == reference);

        public Task<Payment?> GetLatestForCustomerOrderAsync(Guid orderId, Guid customerId)
            => _context.Payments.Include(p => p.Order).ThenInclude(o => o.Customer)
                .Where(p => p.OrderId == orderId && p.Order.CustomerId == customerId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

        public Task<Payment?> GetByIdWithOrderAsync(int paymentId)
            => _context.Payments.Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId);
    }
}
