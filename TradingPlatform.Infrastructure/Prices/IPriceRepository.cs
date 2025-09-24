using TradingPlatform.Domain.Prices;

namespace TradingPlatform.Infrastructure.Prices;

public interface IPriceRepository
{
    Task UpsertDailyAsync(IEnumerable<PriceDaily> items, CancellationToken ct);
    Task<DateOnly?> GetLatestDateAsync(string symbol, CancellationToken ct);
    Task<IReadOnlyList<PriceDaily>> GetSeriesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken ct);
}
