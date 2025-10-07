using System.Threading;
using System.Threading.Tasks;
using Carter;
using Microsoft.AspNetCore.Routing;
using TradingPlatform.Application.Ingestion;

namespace TradingPlatform.Api.Features.Filings;

/// <summary>Filings endpoints (backfill + daily) לפי מבנה Features.</summary>
public sealed class FilingsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // שומר על ה-URLs שהצענו קודם כדי לא לשבור לקוחות
        app.MapPost("/ingestion/filings/backfill", async (int days, FilingsBackfillJob job, CancellationToken ct) =>
        {
            var inserted = await job.RunAsync(days, ct);
            return Results.Ok(new { inserted, days });
        });

        app.MapPost("/ingestion/filings/daily/run", async (FilingsDailyJob job, CancellationToken ct) =>
        {
            var inserted = await job.RunAsync(ct);
            return Results.Ok(new { inserted });
        });
    }
}
