using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.DTOs.Dashboard;
using Application.Exceptions;
using Domain.Enums;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private const int MaxDays = 365;
        private const int MaxCount = 100;

        private readonly IDashboardRepository _repo;

        public DashboardService(IDashboardRepository repo) => _repo = repo;

        public Task<DashboardSummaryDto> GetSummaryAsync(int lowStockThreshold = 5, CancellationToken ct = default)
        {
            if (lowStockThreshold < 0)
                throw new BadRequestException("Low stock threshold cannot be negative.");
            return _repo.GetSummaryAsync(lowStockThreshold, ct);
        }

        public async Task<List<SalesPointDto>> GetSalesAsync(int days = 30, CancellationToken ct = default)
        {
            if (days < 1 || days > MaxDays)
                throw new BadRequestException($"Days must be between 1 and {MaxDays}.");

            var today = DateTime.UtcNow.Date;
            var from = today.AddDays(-(days - 1));
            var data = (await _repo.GetDailySalesAsync(from, today.AddDays(1), ct))
                .ToDictionary(x => x.Date.Date);

            // Fill days without sales with zeros so charts get a continuous series
            return Enumerable.Range(0, days)
                .Select(i => from.AddDays(i))
                .Select(d => data.TryGetValue(d, out var p) ? p : new SalesPointDto { Date = d })
                .ToList();
        }

        public Task<List<TopProductDto>> GetTopProductsAsync(int count = 10, CancellationToken ct = default)
            => _repo.GetTopProductsAsync(ValidateCount(count), ct);

        public Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 5, int count = 20, CancellationToken ct = default)
        {
            if (threshold < 0)
                throw new BadRequestException("Threshold cannot be negative.");
            return _repo.GetLowStockProductsAsync(threshold, ValidateCount(count), ct);
        }

        public async Task<List<OrderStatusCountDto>> GetOrdersByStatusAsync(CancellationToken ct = default)
        {
            var data = (await _repo.GetOrdersByStatusAsync(ct)).ToDictionary(x => x.Status);

            // Always return every status (0 when none) for a stable UI
            return Enum.GetNames<OrderStatus>()
                .Select(s => data.TryGetValue(s, out var d) ? d : new OrderStatusCountDto { Status = s })
                .ToList();
        }

        public Task<List<RecentOrderDto>> GetRecentOrdersAsync(int count = 10, CancellationToken ct = default)
            => _repo.GetRecentOrdersAsync(ValidateCount(count), ct);

        private static int ValidateCount(int count)
        {
            if (count < 1 || count > MaxCount)
                throw new BadRequestException($"Count must be between 1 and {MaxCount}.");
            return count;
        }
    }
}
