using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TradingPlatform.Application.News;
using TradingPlatform.Infrastructure.News;

namespace TradingPlatform.Api.Features.News;

/// <summary>
/// Endpoints to trigger backfill/daily runs and to query latest news.
/// </summary>
public sealed class NewsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/ingestion/news/backfill", async (int days, NewsBackfillJob job, CancellationToken ct) =>
        {
            await job.RunAsync(days <= 0 ? 365 : days, ct);
            return Results.Accepted();
        });

        app.MapPost("/ingestion/news/daily/run", async (NewsIngestionUseCase useCase, CancellationToken ct) =>
        {
            var now = DateTime.UtcNow;
            var from = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(-1);
            await useCase.RunOnceAsync(from, now, ct);
            return Results.Accepted();
        });

        app.MapGet("/news/latest", async (string? symbol, int take, INewsRepository repo, CancellationToken ct) =>
        {
            var t = take <= 0 ? 50 : take;
            var rows = await repo.GetLatestAsync(symbol, t, ct);
            return Results.Ok(rows);
        });
    }
}
