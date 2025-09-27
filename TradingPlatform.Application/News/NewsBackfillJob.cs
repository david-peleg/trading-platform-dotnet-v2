using Microsoft.Extensions.Hosting;

namespace TradingPlatform.Application.News;

/// <summary>
/// One-shot backfill helper for a year window; callable from endpoint.
/// </summary>
public sealed class NewsBackfillJob
{
    private readonly NewsIngestionUseCase _useCase;
    public NewsBackfillJob(NewsIngestionUseCase useCase) => _useCase = useCase;

    /// <summary>Run backfill for the last 'days' days (default 365).</summary>
    public Task RunAsync(int days, CancellationToken ct)
    {
        var to = DateTime.UtcNow;
        var from = to.AddDays(-Math.Max(1, days));
        return _useCase.RunOnceAsync(from, to, ct);
    }
}
