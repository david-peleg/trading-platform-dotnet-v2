using Microsoft.Extensions.Hosting;
using TradingPlatform.Infrastructure.Features;

namespace TradingPlatform.Application.Features;

/// <summary>
/// Periodic job that recomputes recent features for symbols using a rolling window.
/// </summary>
public sealed class FeaturesDailyJob : BackgroundService
{
    private readonly IFeatureService _featureService;
    private readonly IFeatureRepository _featureRepository;

    public FeaturesDailyJob(
        IFeatureService featureService,
        IFeatureRepository featureRepository)
    {
        _featureService = featureService;
        _featureRepository = featureRepository;
    }

    /// <summary>
    /// Runs a single daily aggregation pass for a symbol over the last 30 days.
    /// Caller is expected to iterate symbols externally.
    /// </summary>
    public async Task RunOnceAsync(string symbol, CancellationToken ct)
    {
        var to = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var from = to.AddDays(-30);

        var priceFeatures = await _featureService
            .ComputePricesAsync(symbol, from, to, ct)
            .ConfigureAwait(false);

        var sentimentFeatures = await _featureService
            .ComputeSentimentAsync(symbol, from, to, ct)
            .ConfigureAwait(false);

        var all = priceFeatures.Concat(sentimentFeatures).ToList();
        if (all.Count == 0)
        {
            return;
        }

        await _featureRepository.UpsertBulkAsync(all, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Background loop – left minimal; orchestration of symbols can be added later.
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // No automatic loop yet; orchestration can be added later.
        return Task.CompletedTask;
    }
}
