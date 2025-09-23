// src/TradingPlatform.Api/Features/Symbols/SymbolsEndpoint.cs
using Carter;
using Microsoft.AspNetCore.Http;
using TradingPlatform.Domain.Symbols;
using TradingPlatform.Infrastructure.Symbols;

namespace TradingPlatform.Api.Features.Symbols;

/// <summary>Carter endpoints for Symbols (seed/search/get).</summary>
public sealed class SymbolsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/ingestion/symbols/seed", async (IEnumerable<SeedSymbolDto> body, ISymbolRegistry registry, CancellationToken ct) =>
        {
            var items = body.Select(d => new Symbol(d.Symbol, d.Name, d.Exchange, d.Country, d.Sector, d.IsActive)).ToList();
            await registry.UpsertAsync(items, ct);
            return Results.NoContent();
        });

        app.MapGet("/symbols", async (string? exchange, int? take, ISymbolRegistry registry, CancellationToken ct) =>
        {
            var items = await registry.GetAllAsync(exchange, take.GetValueOrDefault(100), ct);
            return Results.Ok(items);
        });

        app.MapGet("/symbols/search", async (string q, int? take, ISymbolRegistry registry, CancellationToken ct) =>
        {
            var items = await registry.SearchAsync(q, take.GetValueOrDefault(50), ct);
            return Results.Ok(items);
        });
    }

    /// <summary>DTO for seeding symbols.</summary>
    public sealed record SeedSymbolDto(string Symbol, string Name, string Exchange, string Country, string Sector, bool IsActive);
}
