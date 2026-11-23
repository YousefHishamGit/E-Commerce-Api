namespace Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }

        /// <summary>Revenue of all non-cancelled orders.</summary>
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }

        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public int NewCustomersLast30Days { get; set; }
    }
}
