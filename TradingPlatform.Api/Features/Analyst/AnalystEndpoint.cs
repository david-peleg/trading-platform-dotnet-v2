using System;
using System.Threading;
using System.Threading.Tasks;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TradingPlatform.Application.Analyst;
using TradingPlatform.Infrastructure.Analyst;

namespace TradingPlatform.Api.Features.Analyst;

/// <summary>Carter endpoints for analyst ingestion and queries.</summary>
public sealed class AnalystEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/ingestion/analyst/backfill", async (int? days, AnalystIngestionUseCase useCase, HttpContext ctx, CancellationToken ct) =>
        {
            var to = DateTime.UtcNow;
            var from = to.AddDays(-(days ?? 365));
            await useCase.RunOnceAsync(from, to, ct);
            return Results.Ok(new { ok = true, from, to });
        });

        app.MapPost("/ingestion/analyst/daily/run", async (AnalystDailyJob job, CancellationToken ct) =>
        {
            await job.RunAsync(ct);
            return Results.Ok(new { ok = true });
        });

        app.MapGet("/analyst/latest", async (string symbol, int? take, IAnalystReportsRepository repo, CancellationToken ct) =>
        {
            var list = await repo.GetBySymbolAsync(symbol, take.GetValueOrDefault(50), ct);
            return Results.Ok(list);
        });
    }
}
