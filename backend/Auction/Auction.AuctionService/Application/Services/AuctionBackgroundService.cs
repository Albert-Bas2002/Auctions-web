using Auction.AuctionService.Core.Abstractions;
using Microsoft.Extensions.Hosting;

public class AuctionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

    public AuctionBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AuctionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var auctionStatusService = scope.ServiceProvider.GetRequiredService<IAuctionStatusService>();

            try
            {
                await auctionStatusService.AuctionCheckProcess();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing auctions.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Auction background service stopping.");
    }
}
