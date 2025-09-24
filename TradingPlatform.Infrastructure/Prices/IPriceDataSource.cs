using TradingPlatform.Domain.Prices;

namespace TradingPlatform.Infrastructure.Prices;

public interface IPriceDataSource
{
    Task<IReadOnlyList<PriceDaily>> GetDailyAsync(string symbol, DateOnly from, DateOnly to, CancellationToken ct);
}
