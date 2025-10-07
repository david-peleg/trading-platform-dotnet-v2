using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TradingPlatform.Application.Ingestion;

/// <summary>
/// BackgroundService שמריץ את FilingsDailyJob במחזוריות (ברירת מחדל: פעם ביום).
/// </summary>
public sealed class FilingsDailyWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<FilingsDailyWorker> _logger;
    private readonly TimeSpan _interval;

    public FilingsDailyWorker(IServiceProvider sp, ILogger<FilingsDailyWorker> logger, IConfiguration cfg)
    {
        _sp = sp;
        _logger = logger;
        var minutes = cfg.GetValue<int?>("Filings:DailyIntervalMinutes") ?? 1440; // 24h
        _interval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ריצה מידית ואז מחזורית
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<FilingsDailyJob>();
                var inserted = await job.RunAsync(stoppingToken);
                _logger.LogInformation("FilingsDailyWorker ran. Inserted={Inserted}", inserted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FilingsDailyWorker failed.");
            }

            try { await Task.Delay(_interval, stoppingToken); }
            catch (OperationCanceledException) { /* shutdown */ }
        }
    }
}
