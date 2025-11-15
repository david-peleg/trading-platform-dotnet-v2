using TradingPlatform.Domain.Features;

namespace TradingPlatform.Application.Features;

/// <summary>
/// Provides feature computation over price series and attribution data.
/// </summary>
public interface IFeatureService
{
    /// <summary>
    /// Computes technical price features (MA/RSI/Volatility) for the given symbol and date range.
    /// </summary>
    Task<IReadOnlyList<Feature>> ComputePricesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken ct);

    /// <summary>
    /// Computes sentiment features based on attributions for the given symbol and date range.
    /// </summary>
    Task<IReadOnlyList<Feature>> ComputeSentimentAsync(string symbol, DateOnly from, DateOnly to, CancellationToken ct);
}
