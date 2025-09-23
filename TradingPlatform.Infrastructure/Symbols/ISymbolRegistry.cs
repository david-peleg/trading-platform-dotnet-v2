// src/TradingPlatform.Infrastructure/Symbols/ISymbolRegistry.cs
using TradingPlatform.Domain.Symbols;

namespace TradingPlatform.Infrastructure.Symbols;

/// <summary>Symbols registry backed by SQL stored procedures.</summary>
public interface ISymbolRegistry
{
    Task UpsertAsync(IEnumerable<Symbol> items, CancellationToken ct);
    Task<IReadOnlyList<Symbol>> GetAllAsync(string? exchange, int take, CancellationToken ct);
    Task<IReadOnlyList<Symbol>> SearchAsync(string query, int take, CancellationToken ct);
}
