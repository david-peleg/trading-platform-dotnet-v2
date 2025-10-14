using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradingPlatform.Domain.Analyst;

namespace TradingPlatform.Infrastructure.Analyst;

/// <summary>Dummy source returning sample analyst reports; hashes by Symbol|Firm|Title|PublishedAt|Url.</summary>
public sealed class DummyAnalystSource : IAnalystSource
{
    public Task<IReadOnlyList<AnalystReport>> GetAsync(string symbol, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        var items = new List<AnalystReport>();

        // two simple demo rows within range
        var published1 = new DateTime(toUtc.Year, toUtc.Month, Math.Max(1, toUtc.Day - 1), 10, 0, 0, DateTimeKind.Utc);
        var url1 = $"https://dummy.local/{symbol}/note-1";
        items.Add(Make(symbol, "Demo Securities", "Reit. Buy; raise TP", "Reiterate", "Buy", "Buy", 150.00m, "USD", url1, "Dummy", published1, "en"));

        var published2 = new DateTime(fromUtc.Year, fromUtc.Month, Math.Min(28, fromUtc.Day + 1), 9, 0, 0, DateTimeKind.Utc);
        var url2 = $"https://dummy.local/{symbol}/note-2";
        items.Add(Make(symbol, "Alpha Research", "Downgrade to Hold", "Downgrade", "Buy", "Hold", 120.00m, "USD", url2, "Dummy", published2, "en"));

        return Task.FromResult<IReadOnlyList<AnalystReport>>(items);
    }

    private static AnalystReport Make(string symbol, string firm, string title, string action,
        string ratingFrom, string ratingTo, decimal target, string currency, string url, string source,
        DateTime publishedAt, string lang)
    {
        var input = $"{symbol}|{firm}|{title}|{publishedAt:O}|{url}";
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new AnalystReport(symbol, firm, title, action, ratingFrom, ratingTo, target, currency, url, source, publishedAt, hash, lang);
    }
}
