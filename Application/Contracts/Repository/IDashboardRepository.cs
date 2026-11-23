using Application.DTOs.Dashboard;

namespace Application.Contracts.Repository
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryDto> GetSummaryAsync(int lowStockThreshold, CancellationToken ct = default);
        Task<List<SalesPointDto>> GetDailySalesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
        Task<List<TopProductDto>> GetTopProductsAsync(int count, CancellationToken ct = default);
        Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold, int count, CancellationToken ct = default);
        Task<List<OrderStatusCountDto>> GetOrdersByStatusAsync(CancellationToken ct = default);
        Task<List<RecentOrderDto>> GetRecentOrdersAsync(int count, CancellationToken ct = default);
    }
}
