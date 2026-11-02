using Application.DTOs.Common;
using Application.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public interface IOrderService
    {
      
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(CancellationToken cancellationToken = default);
        Task<OrderDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<PaginatedResponseDto<OrderDto>> GetAllOrdersAsync(int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, string newStatus, CancellationToken cancellationToken = default);
        Task<OrderDto> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}

