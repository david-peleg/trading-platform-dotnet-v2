using TradingPlatform.Domain.Prices;

namespace TradingPlatform.Infrastructure.Prices;

/// <summary>Synthetic random-walk data for manual tests.</summary>
public sealed class DummyPriceDataSource : IPriceDataSource
{
    public Task<IReadOnlyList<PriceDaily>> GetDailyAsync(string symbol, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var days = (to.DayNumber - from.DayNumber) + 1;
        if (days <= 0) return Task.FromResult<IReadOnlyList<PriceDaily>>(Array.Empty<PriceDaily>());

        var seed = symbol.Aggregate(17, (a, c) => a * 31 + c);
        var rnd = new Random(seed);
        decimal price = 100m + (rnd.Next(0, 2000) / 10m);

        var list = new List<PriceDaily>(days);
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            var change = (decimal)(rnd.NextDouble() - 0.5) * 2m;
            var open = price;
            var high = open + Math.Abs(change) + (decimal)rnd.NextDouble();
            var low = open - Math.Abs(change) - (decimal)rnd.NextDouble();
            var close = open + change;
            var vol = rnd.Next(100_000, 5_000_000);
            price = Math.Max(1m, close);

            list.Add(new PriceDaily(symbol, d, open, high, low, close, vol, "Dummy"));
        }
        return Task.FromResult<IReadOnlyList<PriceDaily>>(list);
    }
}
