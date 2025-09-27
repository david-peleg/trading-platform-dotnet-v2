using TradingPlatform.Domain.News;

namespace TradingPlatform.Infrastructure.News;

/// <summary>
/// Stored-procedure based access to RawNews storage.
/// </summary>
public interface INewsRepository
{
    /// <summary>Bulk upsert raw news via TVP.</summary>
    Task UpsertBulkAsync(IEnumerable<RawNews> items, CancellationToken ct);

    /// <summary>Get global latest published date (or null if empty).</summary>
    Task<DateTime?> GetLatestDateAsync(CancellationToken ct);

    /// <summary>Get latest news optionally filtered by symbol.</summary>
    Task<IReadOnlyList<RawNews>> GetLatestAsync(string? symbol, int take, CancellationToken ct);
}
