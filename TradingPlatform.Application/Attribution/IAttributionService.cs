using TradingPlatform.Domain.Attribution;
using DomainAttribution = TradingPlatform.Domain.Attribution.Attribution;

namespace TradingPlatform.Application.Attribution;


/// <summary>Links an event to one or more symbols and persists via SP.</summary>
public interface IAttributionService
{
    /// <summary>Link a single item and persist results (SP-Only).</summary>
    Task<IReadOnlyList<DomainAttribution>> LinkAsync(
        ItemType itemType,
        long itemId,
        string? headlineOrText,
        DateTime publishedUtc,
        CancellationToken ct);

    /// <summary>Link multiple items in a batch and persist (SP-Only).</summary>
    Task<IReadOnlyList<DomainAttribution>> LinkBatchAsync(
        ItemType itemType,
        IEnumerable<(long itemId, string? text, DateTime publishedUtc)> items,
        CancellationToken ct);
}
