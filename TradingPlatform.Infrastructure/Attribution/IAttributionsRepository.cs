namespace TradingPlatform.Domain.Attribution;

/// <summary>DB access for Attributions (SP-only).</summary>
public interface IAttributionsRepository
{
    /// <summary>Bulk upsert via dbo.Attributions_InsertBulk (TVP: dbo.AttributionRow).</summary>
    Task InsertBulkAsync(IReadOnlyList<Attribution> rows, CancellationToken ct);

    Task<IReadOnlyList<Attribution>> GetBySymbolAsync(string symbol, short? horizonD, int take, CancellationToken ct);

    Task<IReadOnlyList<Attribution>> GetForItemAsync(ItemType itemType, long itemId, CancellationToken ct);
}
