using Microsoft.Extensions.Logging;
using TradingPlatform.Infrastructure.Symbols;
using TradingPlatform.Infrastructure.Prices;

namespace TradingPlatform.Application.Prices;

/// <summary>Backfills daily prices for all symbols.</summary>
public sealed class PricesBackfillJob
{
    private readonly ILogger<PricesBackfillJob> _logger;
    private readonly ISymbolRegistry _symbols;
    private readonly IPriceRepository _repo;
    private readonly IPriceDataSource _source;

    public PricesBackfillJob(
        ILogger<PricesBackfillJob> logger,
        ISymbolRegistry symbols,
        IPriceRepository repo,
        IPriceDataSource source)
    {
        _logger = logger;
        _symbols = symbols;
        _repo = repo;
        _source = source;
    }

    public async Task RunOnceAsync(int days, CancellationToken ct)
    {
        var to = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var backfillDays = days > 0 ? days : 365;
        var defaultFrom = to.AddDays(-backfillDays);

        var all = await _symbols.GetAllAsync(exchange: null, take: 10_000, ct);

        foreach (var s in all)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var from = defaultFrom;
                var latest = await _repo.GetLatestDateAsync(s.SymbolCode, ct);
                if (latest is DateOnly d)
                {
                    var next = d.AddDays(1);
                    if (next > from) from = next;
                }
                if (from > to) continue;

                var rows = await _source.GetDailyAsync(s.SymbolCode, from, to, ct);
                if (rows.Count > 0)
                {
                    await _repo.UpsertDailyAsync(rows, ct);
                    _logger.LogInformation("Backfilled {Count} rows for {Symbol}", rows.Count, s.SymbolCode);
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Backfill failed for {Symbol}", s.SymbolCode);
            }
        }
    }
}
