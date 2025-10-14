using System;
using System.Threading;
using System.Threading.Tasks;

namespace TradingPlatform.Application.Analyst;

/// <summary>Backfills analyst reports for the last year (UtcNow-365d → UtcNow).</summary>
public sealed class AnalystBackfillJob
{
    private readonly AnalystIngestionUseCase _useCase;

    public AnalystBackfillJob(AnalystIngestionUseCase useCase) => _useCase = useCase;

    public Task RunAsync(CancellationToken ct)
    {
        var to = DateTime.UtcNow;
        var from = to.AddDays(-365);
        return _useCase.RunOnceAsync(from, to, ct);
    }
}
