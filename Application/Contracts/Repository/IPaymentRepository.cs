using Domain.Entities;

namespace Application.Contracts.Repository
{
    public interface IPaymentRepository
    {
        /// <summary>Tracked payment (with its Order) matching the gateway reference, or null.</summary>
        Task<Payment?> GetByGatewayReferenceAsync(string reference);

        /// <summary>Tracked latest payment of an order owned by the customer (with Order and Customer), or null.</summary>
        Task<Payment?> GetLatestForCustomerOrderAsync(Guid orderId, Guid customerId);

        /// <summary>Tracked payment by id (with Order), or null.</summary>
        Task<Payment?> GetByIdWithOrderAsync(int paymentId);
    }
}
