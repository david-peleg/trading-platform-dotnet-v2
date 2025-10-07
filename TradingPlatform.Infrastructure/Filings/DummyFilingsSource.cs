using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradingPlatform.Domain.Filings;

namespace TradingPlatform.Infrastructure.Filings;

/// <summary>Dev dummy source producing deterministic sample filings.</summary>
public sealed class DummyFilingsSource : IFilingsSource
{
    public Task<IReadOnlyList<Filing>> FetchAsync(int daysBack, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Filing>>(Generate(DateTime.UtcNow.AddDays(-daysBack), DateTime.UtcNow));

    public Task<IReadOnlyList<Filing>> FetchSinceAsync(DateTime sinceUtc, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Filing>>(Generate(sinceUtc, DateTime.UtcNow));

    private static List<Filing> Generate(DateTime fromUtc, DateTime toUtc)
    {
        var list = new List<Filing>();
        var dates = new[] { toUtc.AddDays(-7), toUtc.AddDays(-30), toUtc.AddDays(-90) };
        foreach (var (sym, type) in new[] { ("AAPL", "10-Q"), ("MSFT", "10-K"), ("NVDA", "8-K") })
        {
            foreach (var d in dates)
            {
                if (d < fromUtc || d > toUtc) continue;
                var url = $"https://example.local/{sym}/{type}/{d:yyyyMMdd}";
                var src = "DUMMY";
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{sym}|{type}|{url}|{src}|{d.Ticks}"));

                list.Add(new Filing(
                    Symbol: sym,
                    FilingType: type,
                    PeriodStart: null,
                    PeriodEnd: null,
                    Url: url,
                    Source: src,
                    PublishedAtUtc: DateTime.SpecifyKind(d, DateTimeKind.Utc),
                    DocHash: hash,
                    Lang: "en"));
            }
        }
        return list;
    }
}
