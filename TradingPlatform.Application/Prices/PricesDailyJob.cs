using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingPlatform.Infrastructure.Symbols;
using TradingPlatform.Infrastructure.Prices;

namespace TradingPlatform.Application.Prices;

/// <summary>Daily refresh for last 2 days for all symbols.</summary>
public sealed class PricesDailyJob : BackgroundService
{
    private readonly ILogger<PricesDailyJob> _logger;
    private readonly ISymbolRegistry _symbols;
    private readonly IPriceRepository _repo;
    private readonly IPriceDataSource _source;
    private int _running;

    public PricesDailyJob(
        ILogger<PricesDailyJob> logger,
        ISymbolRegistry symbols,
        IPriceRepository repo,
        IPriceDataSource source)
    {
        _logger = logger;
        _symbols = symbols;
        _repo = repo;
        _source = source;
    }

    public async Task RunOnceAsync(CancellationToken ct)
    {
        if (Interlocked.Exchange(ref _running, 1) == 1) return;
        try
        {
            var to = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var from = to.AddDays(-2);

            var list = await _symbols.GetAllAsync(exchange: null, take: 10_000, ct);
            foreach (var s in list)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var rows = await _source.GetDailyAsync(s.SymbolCode, from, to, ct);
                    if (rows.Count > 0)
                    {
                        await _repo.UpsertDailyAsync(rows, ct);
                        _logger.LogInformation("Daily updated {Count} rows for {Symbol}", rows.Count, s.SymbolCode);
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Daily update failed for {Symbol}", s.SymbolCode);
                }
            }
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunOnceAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                if (stoppingToken.IsCancellationRequested) break;
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
        }
    }
}
