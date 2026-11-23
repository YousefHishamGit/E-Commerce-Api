using Application.Common;
using Application.Contracts.Services;

namespace E_Commerce.Service
{
    /// <summary>
    /// Every minute cancels online-payment orders that were not paid in time, releasing the reserved stock
    /// and coupon usage, so abandoned checkouts do not block inventory.
    /// </summary>
    public class UnpaidOrderCleanupService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly PaymobSettings _settings;
        private readonly ILogger<UnpaidOrderCleanupService> _logger;

        public UnpaidOrderCleanupService(IServiceScopeFactory scopeFactory, PaymobSettings settings, ILogger<UnpaidOrderCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            while (await WaitAsync(timer, stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();   // fresh DbContext per run
                    var orders = scope.ServiceProvider.GetRequiredService<IOrderService>();
                    await orders.CancelExpiredUnpaidOrdersAsync(_settings.UnpaidOrderMinutes, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unpaid order cleanup failed; will retry on the next tick.");
                }
            }
        }

        private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken ct)
        {
            try { return await timer.WaitForNextTickAsync(ct); }
            catch (OperationCanceledException) { return false; }
        }
    }
}
