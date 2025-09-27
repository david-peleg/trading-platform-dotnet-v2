using TradingPlatform.Domain.News;

namespace TradingPlatform.Infrastructure.News;

/// <summary>
/// Streaming source of raw news items for a given time window.
/// </summary>
public interface INewsSource
{
    /// <summary>Iterate news in [fromUtc,toUtc].</summary>
    IAsyncEnumerable<RawNews> ReadAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct);
}
