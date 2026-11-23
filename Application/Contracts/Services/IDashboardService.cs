using Application.DTOs.Dashboard;

namespace Application.Contracts.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(int lowStockThreshold = 5, CancellationToken ct = default);
        Task<List<SalesPointDto>> GetSalesAsync(int days = 30, CancellationToken ct = default);
        Task<List<TopProductDto>> GetTopProductsAsync(int count = 10, CancellationToken ct = default);
        Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 5, int count = 20, CancellationToken ct = default);
        Task<List<OrderStatusCountDto>> GetOrdersByStatusAsync(CancellationToken ct = default);
        Task<List<RecentOrderDto>> GetRecentOrdersAsync(int count = 10, CancellationToken ct = default);
    }
}
