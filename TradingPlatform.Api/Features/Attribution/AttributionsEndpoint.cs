using Carter;
using Microsoft.AspNetCore.Http;
using TradingPlatform.Domain.Attribution;

namespace TradingPlatform.Api.Features.Attribution;

/// <summary>HTTP endpoints for Attributions (read-only).</summary>
public sealed class AttributionsEndpoint : CarterModule
{
    public AttributionsEndpoint() : base("/attributions") { }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/by-symbol/{symbol}", async (
            string symbol,
            short? horizon,
            int? take,
            IAttributionsRepository repo,
            CancellationToken ct) =>
        {
            var res = await repo.GetBySymbolAsync(symbol, horizon, take ?? 50, ct);
            return Results.Ok(res);
        });

        app.MapGet("/for-item/{type:int}/{id:long}", async (
            int type,
            long id,
            IAttributionsRepository repo,
            CancellationToken ct) =>
        {
            var res = await repo.GetForItemAsync((ItemType)type, id, ct);
            return Results.Ok(res);
        });
    }
}
