using TradingPlatform.Domain.Features;

namespace TradingPlatform.Infrastructure.Features;

/// <summary>
/// Provides DB access for reading and writing computed features.
/// </summary>
public interface IFeatureRepository
{
    /// <summary>
    /// Upserts a batch of features using a TVP-backed stored procedure.
    /// </summary>
    Task UpsertBulkAsync(IEnumerable<Feature> items, CancellationToken ct);

    /// <summary>
    /// Deletes features for a symbol and date range, optionally filtered by feature name prefix.
    /// </summary>
    Task DeleteRangeAsync(string symbol, DateOnly from, DateOnly to, string? prefix, CancellationToken ct);

    /// <summary>
    /// Gets features for a symbol and date range, optionally filtered by feature name prefix.
    /// </summary>
    Task<IReadOnlyList<Feature>> GetBySymbolAsync(string symbol, DateOnly from, DateOnly to, string? prefix, CancellationToken ct);
}
