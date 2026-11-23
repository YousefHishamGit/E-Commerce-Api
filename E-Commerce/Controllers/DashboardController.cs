using Application.Contracts.Services;
using Application.DTOs.Common;
using Application.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardService _dashboard;

        public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

        /// <summary>Key numbers: customers, products, orders, revenue, today sales, stock alerts.</summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponseDto<DashboardSummaryDto>>> GetSummary(
            [FromQuery] int lowStockThreshold = 5, CancellationToken ct = default)
            => Ok(ApiResponseDto<DashboardSummaryDto>.Ok(await _dashboard.GetSummaryAsync(lowStockThreshold, ct)));

        /// <summary>Daily orders count and revenue for the last N days (default 30, max 365).</summary>
        [HttpGet("sales")]
        public async Task<ActionResult<ApiResponseDto<List<SalesPointDto>>>> GetSales(
            [FromQuery] int days = 30, CancellationToken ct = default)
            => Ok(ApiResponseDto<List<SalesPointDto>>.Ok(await _dashboard.GetSalesAsync(days, ct)));

        /// <summary>Best-selling products by quantity (cancelled orders excluded).</summary>
        [HttpGet("top-products")]
        public async Task<ActionResult<ApiResponseDto<List<TopProductDto>>>> GetTopProducts(
            [FromQuery] int count = 10, CancellationToken ct = default)
            => Ok(ApiResponseDto<List<TopProductDto>>.Ok(await _dashboard.GetTopProductsAsync(count, ct)));

        /// <summary>Products whose stock is at or below the threshold.</summary>
        [HttpGet("low-stock")]
        public async Task<ActionResult<ApiResponseDto<List<LowStockProductDto>>>> GetLowStock(
            [FromQuery] int threshold = 5, [FromQuery] int count = 20, CancellationToken ct = default)
            => Ok(ApiResponseDto<List<LowStockProductDto>>.Ok(await _dashboard.GetLowStockProductsAsync(threshold, count, ct)));

        /// <summary>Number of orders per status.</summary>
        [HttpGet("orders-by-status")]
        public async Task<ActionResult<ApiResponseDto<List<OrderStatusCountDto>>>> GetOrdersByStatus(CancellationToken ct = default)
            => Ok(ApiResponseDto<List<OrderStatusCountDto>>.Ok(await _dashboard.GetOrdersByStatusAsync(ct)));

        /// <summary>Latest orders.</summary>
        [HttpGet("recent-orders")]
        public async Task<ActionResult<ApiResponseDto<List<RecentOrderDto>>>> GetRecentOrders(
            [FromQuery] int count = 10, CancellationToken ct = default)
            => Ok(ApiResponseDto<List<RecentOrderDto>>.Ok(await _dashboard.GetRecentOrdersAsync(count, ct)));
    }
}
