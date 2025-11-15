using TradingPlatform.Domain.Attribution;
using TradingPlatform.Domain.Features;
using TradingPlatform.Domain.Prices;
using TradingPlatform.Infrastructure.Prices;

namespace TradingPlatform.Application.Features;

/// <summary>
/// Default implementation of IFeatureService using price and attribution repositories.
/// </summary>
public sealed class FeatureService : IFeatureService
{
    private readonly IPriceRepository _priceRepository;
    private readonly IAttributionsRepository _attributionRepository;

    public FeatureService(
        IPriceRepository priceRepository,
        IAttributionsRepository attributionRepository)
    {
        _priceRepository = priceRepository;
        _attributionRepository = attributionRepository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Feature>> ComputePricesAsync(
        string symbol,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        var prices = await _priceRepository.GetSeriesAsync(symbol, from, to, ct).ConfigureAwait(false);
        var ordered = prices.OrderBy(p => p.Dt).ToList();

        var closes = ordered
            .Select(p => (dynamic)new { p.Dt, Close = (double)p.Close })
            .ToList();

        var features = new List<Feature>();

        AddMovingAverageFeatures(closes, 20, symbol, features);
        AddMovingAverageFeatures(closes, 50, symbol, features);
        AddRsiFeatures(closes, 14, symbol, features);
        AddVolatilityFeatures(closes, 20, symbol, features);

        return features;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Feature>> ComputeSentimentAsync(
        string symbol,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        var attributions = await _attributionRepository
            .GetBySymbolAsync(symbol, from, to, ct)
            .ConfigureAwait(false);

        var grouped = attributions
            .GroupBy(a => a.Dt)
            .Select(g =>
            {
                var avg = g.Average(x => x.Direction * x.Confidence);
                return new Feature(
                    symbol,
                    g.Key,
                    "SENT_D1",
                    avg,
                    null);
            })
            .ToList();

        return grouped;
    }

    private static void AddMovingAverageFeatures(
        List<dynamic> closes,
        int window,
        string symbol,
        List<Feature> output)
    {
        if (closes.Count < window)
        {
            return;
        }

        double sum = 0;
        for (var i = 0; i < closes.Count; i++)
        {
            sum += closes[i].Close;

            if (i >= window)
            {
                sum -= closes[i - window].Close;
            }

            if (i >= window - 1)
            {
                var dt = closes[i].Dt;
                var value = sum / window;
                var name = $"MA_{window}";

                output.Add(new Feature(symbol, dt, name, value, (short)window));
            }
        }
    }

    private static void AddRsiFeatures(
        List<dynamic> closes,
        int period,
        string symbol,
        List<Feature> output)
    {
        if (closes.Count <= period)
        {
            return;
        }

        var gains = new double[closes.Count];
        var losses = new double[closes.Count];

        for (var i = 1; i < closes.Count; i++)
        {
            var change = closes[i].Close - closes[i - 1].Close;
            if (change > 0)
            {
                gains[i] = change;
                losses[i] = 0;
            }
            else
            {
                gains[i] = 0;
                losses[i] = -change;
            }
        }

        double avgGain = 0;
        double avgLoss = 0;

        for (var i = 1; i <= period; i++)
        {
            avgGain += gains[i];
            avgLoss += losses[i];
        }

        avgGain /= period;
        avgLoss /= period;

        for (var i = period + 1; i < closes.Count; i++)
        {
            avgGain = (avgGain * (period - 1) + gains[i]) / period;
            avgLoss = (avgLoss * (period - 1) + losses[i]) / period;

            if (avgLoss <= 0)
            {
                continue;
            }

            var rs = avgGain / avgLoss;
            var rsi = 100.0 - 100.0 / (1.0 + rs);

            var dt = closes[i].Dt;
            output.Add(new Feature(symbol, dt, $"RSI_{period}", rsi, (short)period));
        }
    }

    private static void AddVolatilityFeatures(
        List<dynamic> closes,
        int window,
        string symbol,
        List<Feature> output)
    {
        if (closes.Count <= window)
        {
            return;
        }

        var returns = new double[closes.Count];
        for (var i = 1; i < closes.Count; i++)
        {
            if (closes[i - 1].Close <= 0)
            {
                returns[i] = 0;
            }
            else
            {
                returns[i] = Math.Log(closes[i].Close / closes[i - 1].Close);
            }
        }

        for (var i = window; i < closes.Count; i++)
        {
            var slice = returns[(i - window + 1)..(i + 1)];
            var mean = slice.Average();
            var variance = slice.Select(r => (r - mean) * (r - mean)).Average();
            var stdev = Math.Sqrt(variance);

            var dt = closes[i].Dt;
            output.Add(new Feature(symbol, dt, $"VOL_{window}", stdev, (short)window));
        }
    }
}
