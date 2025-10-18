using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TradingPlatform.Domain.Attribution;

// Alias לטיפוס הדומיין כדי למנוע התנגשויות שם
using DomainAttribution = TradingPlatform.Domain.Attribution.Attribution;

namespace TradingPlatform.Infrastructure.Attributions;

/// <summary>
/// Heuristic linker (match ticker-like tokens + simple sentiment) and persist via repository (SP-only).
/// </summary>
public sealed class AttributionService : IAttributionService
{
    // עד שניישר API של ISymbolRegistry – משתמשים ב-regex פשוט לטיקר
    private readonly IAttributionsRepository _repo;
    private readonly ILogger<AttributionService> _log;

    private static readonly Regex TokenRx =
        new(@"(?<!\p{L})(?<tok>[A-Z]{1,6})(?!\p{L})", RegexOptions.Compiled);

    private static readonly string[] Pos = { "beat", "beats", "surge", "upgrade", "raised", "record", "strong", "profit", "outperform" };
    private static readonly string[] Neg = { "miss", "misses", "drop", "downgrade", "cut", "lawsuit", "probe", "weak", "loss" };

    public AttributionService(IAttributionsRepository repo, ILogger<AttributionService> log)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public async Task<IReadOnlyList<DomainAttribution>> LinkAsync(
        ItemType itemType, long itemId, string? headlineOrText, DateTime publishedUtc, CancellationToken ct)
    {
        var list = Compute(itemType, itemId, headlineOrText, publishedUtc);
        if (list.Count == 0) return Array.Empty<DomainAttribution>();
        await _repo.InsertBulkAsync(list, ct);
        return list;
    }

    public async Task<IReadOnlyList<DomainAttribution>> LinkBatchAsync(
        ItemType itemType, IEnumerable<(long itemId, string? text, DateTime publishedUtc)> items, CancellationToken ct)
    {
        var results = new List<DomainAttribution>(256);
        foreach (var (id, text, dt) in items)
            results.AddRange(Compute(itemType, id, text, dt));

        if (results.Count > 0)
            await _repo.InsertBulkAsync(results, ct);

        return results;
    }

    private static short PickHorizon(ItemType t) => t switch
    {
        ItemType.News => 7,
        ItemType.Filing => 30,
        ItemType.Analyst => 30,
        _ => 7
    };

    private List<DomainAttribution> Compute(ItemType itemType, long itemId, string? text, DateTime _)
    {
        if (string.IsNullOrWhiteSpace(text)) return new();

        // 1) זיהוי טיקר בסיסי לפי regex בלבד (מניעת תלות ב-ISymbolRegistry עד לאיחוד ה-API)
        var matches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match m in TokenRx.Matches(text.ToUpperInvariant()))
        {
            var tok = m.Groups["tok"].Value;
            if (tok.Length is >= 1 and <= 6) matches.Add(tok);
        }
        if (matches.Count == 0) return new();

        // 2) Sentiment → direction/confidence
        var lower = text.ToLowerInvariant();
        int pos = Pos.Count(p => lower.Contains(p));
        int neg = Neg.Count(n => lower.Contains(n));
        short direction = (short)(pos > neg ? +1 : (neg > pos ? -1 : 0));
        double confidence = Math.Clamp((pos + neg) switch { >= 3 => 0.8, 2 => 0.6, 1 => 0.5, _ => 0.4 }, 0, 1);
        short horizon = PickHorizon(itemType);

        return matches
            .Select(s => new DomainAttribution(itemType, itemId, s, direction, confidence, horizon))
            .ToList();
    }
}
