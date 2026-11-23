using Application.Contracts.Repository;
using Application.DTOs.Dashboard;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context) => _context = context;

        public async Task<DashboardSummaryDto> GetSummaryAsync(int lowStockThreshold, CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            var last30 = today.AddDays(-30);

            var validOrders = _context.Orders.AsNoTracking().Where(o => o.Status != OrderStatus.Cancelled);

            var revenue = await validOrders.SumAsync(o => (decimal?)o.TotalPrice, ct) ?? 0m;
            var validCount = await validOrders.CountAsync(ct);
            var todayOrders = validOrders.Where(o => o.CreatedAt >= today);

            return new DashboardSummaryDto
            {
                TotalCustomers = await _context.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Customer, ct),
                TotalProducts = await _context.Products.AsNoTracking().CountAsync(ct),
                TotalOrders = await _context.Orders.AsNoTracking().CountAsync(ct),
                PendingOrders = await _context.Orders.AsNoTracking().CountAsync(o => o.Status == OrderStatus.Pending, ct),
                LowStockProducts = await _context.Products.AsNoTracking()
                    .CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= lowStockThreshold, ct),
                OutOfStockProducts = await _context.Products.AsNoTracking().CountAsync(p => p.StockQuantity <= 0, ct),
                TotalRevenue = revenue,
                AverageOrderValue = validCount == 0 ? 0m : Math.Round(revenue / validCount, 2),
                TodayOrders = await todayOrders.CountAsync(ct),
                TodayRevenue = await todayOrders.SumAsync(o => (decimal?)o.TotalPrice, ct) ?? 0m,
                NewCustomersLast30Days = await _context.Users.AsNoTracking()
                    .CountAsync(u => u.Role == UserRole.Customer && u.CreatedAt >= last30, ct)
            };
        }

        public async Task<List<SalesPointDto>> GetDailySalesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
        {
            return await _context.Orders.AsNoTracking()
                .Where(o => o.Status != OrderStatus.Cancelled && o.CreatedAt >= fromUtc && o.CreatedAt < toUtc)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new SalesPointDto
                {
                    Date = g.Key,
                    OrdersCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToListAsync(ct);
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(int count, CancellationToken ct = default)
        {
            return await _context.OrderItems.AsNoTracking()
                .Where(i => i.Order.Status != OrderStatus.Cancelled)
                .GroupBy(i => new { i.ProductId, i.Product.Name, i.Product.Photo })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    Photo = g.Key.Photo,
                    QuantitySold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Quantity * i.UnitPrice)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(count)
                .ToListAsync(ct);
        }

        public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold, int count, CancellationToken ct = default)
        {
            return await _context.Products.AsNoTracking()
                .Where(p => p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .Take(count)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    CategoryName = p.Category.Name,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync(ct);
        }

        public async Task<List<OrderStatusCountDto>> GetOrdersByStatusAsync(CancellationToken ct = default)
        {
            var rows = await _context.Orders.AsNoTracking()
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return rows.Select(r => new OrderStatusCountDto { Status = r.Status.ToString(), Count = r.Count }).ToList();
        }

        public async Task<List<RecentOrderDto>> GetRecentOrdersAsync(int count, CancellationToken ct = default)
        {
            var rows = await _context.Orders.AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .Select(o => new
                {
                    o.Id,
                    Name = o.Customer.FirstName + " " + o.Customer.LastName,
                    o.Status,
                    o.TotalPrice,
                    Items = o.OrderItems.Count,
                    o.CreatedAt
                })
                .ToListAsync(ct);

            return rows.Select(r => new RecentOrderDto
            {
                OrderId = r.Id,
                CustomerName = r.Name,
                Status = r.Status.ToString(),
                TotalPrice = r.TotalPrice,
                ItemsCount = r.Items,
                CreatedAt = r.CreatedAt
            }).ToList();
        }
    }
}
