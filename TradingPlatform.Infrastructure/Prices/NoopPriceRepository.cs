using TradingPlatform.Domain.Prices;

namespace TradingPlatform.Infrastructure.Prices;

/// <summary>
/// No-op implementation for IPriceRepository (לשימוש במצב Noop / ללא DB).
/// </summary>
public sealed class NoopPriceRepository : IPriceRepository
{
    public Task UpsertDailyAsync(IEnumerable<PriceDaily> items, CancellationToken ct)
        => Task.CompletedTask;

    public Task<DateOnly?> GetLatestDateAsync(string symbol, CancellationToken ct)
        => Task.FromResult<DateOnly?>(null);

    public Task<IReadOnlyList<PriceDaily>> GetSeriesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<PriceDaily>>(Array.Empty<PriceDaily>());
}
