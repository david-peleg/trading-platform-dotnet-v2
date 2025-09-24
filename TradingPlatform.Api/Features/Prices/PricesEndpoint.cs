using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // For [FromServices]/[FromQuery]
using TradingPlatform.Application.Prices;
using TradingPlatform.Infrastructure.Prices;

namespace TradingPlatform.Api.Features.Prices;

public sealed class PricesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/ingestion/prices/backfill",
            async ([FromQuery] int? days,
                   [FromServices] PricesBackfillJob job,
                   CancellationToken ct) =>
            {
                var d = days.GetValueOrDefault(365);
                await job.RunOnceAsync(d, ct);
                return Results.Ok(new { status = "ok", days = d });
            });

        app.MapPost("/ingestion/prices/daily/run",
            async ([FromServices] PricesDailyJob job,
                   CancellationToken ct) =>
            {
                await job.RunOnceAsync(ct);
                return Results.Ok(new { status = "ok" });
            });

        app.MapGet("/prices/series",
            async ([FromQuery] string symbol,
                   [FromQuery] int? days,
                   [FromServices] IPriceRepository repo,
                   CancellationToken ct) =>
            {
                var to = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                var span = days.GetValueOrDefault(30);
                var from = to.AddDays(-span);
                var rows = await repo.GetSeriesAsync(symbol, from, to, ct);
                return Results.Ok(rows);
            });
    }
}
