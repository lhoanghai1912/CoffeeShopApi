using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoffeeShopApi.Services;

/// <summary>
/// Background Service tự động cập nhật IsActive của vouchers dựa trên StartDate và EndDate
/// Chạy định kỳ mỗi 1 giờ
/// </summary>
public class VoucherStatusUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<VoucherStatusUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromHours(1); // Chạy mỗi 1 giờ

    public VoucherStatusUpdateService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<VoucherStatusUpdateService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VoucherStatusUpdateService started at: {time}", DateTime.Now);

        // Chạy ngay lần đầu khi service khởi động
        await UpdateVoucherStatusAsync(stoppingToken);

        // Sau đó chạy định kỳ theo interval
        using var timer = new PeriodicTimer(_updateInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await UpdateVoucherStatusAsync(stoppingToken);
        }

        _logger.LogInformation("VoucherStatusUpdateService stopped at: {time}", DateTime.Now);
    }

    private async Task UpdateVoucherStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting voucher status update at: {time}", DateTime.Now);

            // Tạo scope mới để resolve scoped services (DbContext, VoucherService)
            using var scope = _serviceScopeFactory.CreateScope();
            var voucherService = scope.ServiceProvider.GetRequiredService<IVoucherService>();

            var updateCount = await voucherService.UpdateVoucherActiveStatusAsync();

            if (updateCount > 0)
            {
                _logger.LogInformation("Updated {count} vouchers at: {time}", updateCount, DateTime.Now);
            }
            else
            {
                _logger.LogDebug("No vouchers needed update at: {time}", DateTime.Now);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating voucher status at: {time}", DateTime.Now);
        }
    }
}
