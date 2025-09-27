using Microsoft.Extensions.Hosting;

namespace TradingPlatform.Application.News;

/// <summary>
/// Daily update job: yesterday 00:00Z → now. Can be scheduled or invoked via endpoint.
/// </summary>
public sealed class NewsDailyJob : BackgroundService
{
    private readonly NewsIngestionUseCase _useCase;
    public NewsDailyJob(NewsIngestionUseCase useCase) => _useCase = useCase;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run once on startup (optional); scheduling can be added later (cron/Timer).
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(-1);
        await _useCase.RunOnceAsync(from, now, stoppingToken);
    }
}
