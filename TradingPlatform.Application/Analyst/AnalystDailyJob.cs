using System;
using System.Threading;
using System.Threading.Tasks;

namespace TradingPlatform.Application.Analyst;

/// <summary>Runs daily increment: yesterday 00:00Z → now.</summary>
public sealed class AnalystDailyJob
{
    private readonly AnalystIngestionUseCase _useCase;

    public AnalystDailyJob(AnalystIngestionUseCase useCase) => _useCase = useCase;

    public Task RunAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.AddDays(-1).Year, now.AddDays(-1).Month, now.AddDays(-1).Day, 0, 0, 0, DateTimeKind.Utc);
        return _useCase.RunOnceAsync(from, now, ct);
    }
}
