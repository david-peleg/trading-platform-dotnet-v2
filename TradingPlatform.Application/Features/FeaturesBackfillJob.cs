using TradingPlatform.Domain.Features;
using TradingPlatform.Infrastructure.Features;

namespace TradingPlatform.Application.Features;

/// <summary>
/// Orchestrates full recomputation of features for a symbol over a historical window.
/// </summary>
public sealed class FeaturesBackfillJob
{
    private readonly IFeatureService _featureService;
    private readonly IFeatureRepository _featureRepository;

    public FeaturesBackfillJob(
        IFeatureService featureService,
        IFeatureRepository featureRepository)
    {
        _featureService = featureService;
        _featureRepository = featureRepository;
    }

    /// <summary>
    /// Deletes existing features for the range and recomputes price and sentiment features.
    /// </summary>
    public async Task RunAsync(string symbol, int days, string? prefix, CancellationToken ct)
    {
        var to = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var from = to.AddDays(-days);

        await _featureRepository.DeleteRangeAsync(symbol, from, to, prefix, ct).ConfigureAwait(false);

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
}
