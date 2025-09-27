// src/TradingPlatform.Infrastructure/Symbols/ISymbolRegistry.cs
using TradingPlatform.Domain.Symbols;

namespace TradingPlatform.Infrastructure.Symbols;

/// <summary>Symbols registry backed by SQL stored procedures.</summary>
public interface ISymbolRegistry
{
    Task UpsertAsync(IEnumerable<Symbol> items, CancellationToken ct);
    Task<IReadOnlyList<Symbol>> GetAllAsync(string? exchange, int take, CancellationToken ct);
    Task<IReadOnlyList<Symbol>> SearchAsync(string query, int take, CancellationToken ct);

    /// <summary>
    /// Try to match a ticker mentioned in free text (e.g., "(AAPL)" or whole-word match).
    /// Returns the matched ticker or null if none found.
    /// </summary>
    Task<string?> TryMatchAsync(string text, CancellationToken ct);
}
