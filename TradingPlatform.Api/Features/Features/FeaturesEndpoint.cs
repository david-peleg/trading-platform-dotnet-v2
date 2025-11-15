using Carter;
using TradingPlatform.Application.Features;
using TradingPlatform.Domain.Features;
using TradingPlatform.Infrastructure.Features;

namespace TradingPlatform.Api.Features.Features;

/// <summary>
/// Carter endpoints for recomputing and querying features.
/// </summary>
public sealed class FeaturesEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        // POST /features/recompute?symbol=AAPL&days=365&prefix=MA_
        app.MapPost("/features/recompute", async (
            string symbol,
            int days,
            string? prefix,
            FeaturesBackfillJob job,
            CancellationToken ct) =>
        {
            await job.RunAsync(symbol, days, prefix, ct);
            return Results.Ok();
        });

        // POST /features/daily/run?symbol=AAPL
        app.MapPost("/features/daily/run", async (
            string symbol,
            FeaturesDailyJob job,
            CancellationToken ct) =>
        {
            await job.RunOnceAsync(symbol, ct);
            return Results.Ok();
        });

        // GET /features?symbol=AAPL&days=90&prefix=MA_
        app.MapGet("/features", async (
            string symbol,
            int days,
            string? prefix,
            IFeatureRepository repository,
            CancellationToken ct) =>
        {
            var to = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var from = to.AddDays(-days);

            IReadOnlyList<Feature> features =
                await repository.GetBySymbolAsync(symbol, from, to, prefix, ct);

            return Results.Ok(features);
        });
    }
}
