// + using TradingPlatform.Infrastructure.Symbols;   // במקום Domain.Symbols
using TradingPlatform.Infrastructure.Symbols;
using TradingPlatform.Domain.Filings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Security.Cryptography;
using System.Text;

namespace TradingPlatform.Infrastructure.Filings;

public sealed class EdgarRssFilingsSource : IFilingsSource
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISymbolRegistry _symbols;   // <-- Infrastructure.Symbols
    private readonly EdgarOptions _opt;
    private readonly ILogger<EdgarRssFilingsSource> _logger;

    public EdgarRssFilingsSource(
        IHttpClientFactory httpClientFactory,
        ISymbolRegistry symbols,                          // <-- חתימה קיימת
        IOptions<EdgarOptions> options,
        ILogger<EdgarRssFilingsSource> logger)
    {
        _httpClientFactory = httpClientFactory;
        _symbols = symbols;
        _opt = options.Value;
        _logger = logger;
    }

    public Task<IReadOnlyList<Filing>> FetchAsync(int daysBack, CancellationToken ct)
        => FetchSinceAsync(DateTime.UtcNow.AddDays(-daysBack), ct);

    public async Task<IReadOnlyList<Filing>> FetchSinceAsync(DateTime sinceUtc, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(nameof(EdgarRssFilingsSource));
        using var resp = await client.GetAsync(_opt.FeedUrl, ct);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var reader = XmlReader.Create(stream);
        var feed = SyndicationFeed.Load(reader);

        var results = new List<Filing>();

        foreach (var item in feed.Items)
        {
            var published = (item.LastUpdatedTime != DateTimeOffset.MinValue
                                ? item.LastUpdatedTime.UtcDateTime
                                : item.PublishDate.UtcDateTime);
            if (published < sinceUtc) continue;

            var title = item.Title?.Text ?? string.Empty;
            var summary = item.Summary?.Text ?? string.Empty;
            var blob = $"{title} {summary}";

            // NEW: חתימה מחזירה string?
            var matched = await _symbols.TryMatchAsync(blob, ct);
            if (string.IsNullOrWhiteSpace(matched))
                continue;
            var symbol = matched!;

            var type = GuessType(title, item.Categories);
            var url = item.Links?.FirstOrDefault()?.Uri?.ToString();
            if (string.IsNullOrWhiteSpace(url)) continue;

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{symbol}|{type}|{url}|EDGAR|{published.Ticks}"));

            results.Add(new Filing(
                Symbol: symbol,
                FilingType: type,
                PeriodStart: null,
                PeriodEnd: null,
                Url: url!,
                Source: "EDGAR",
                PublishedAtUtc: DateTime.SpecifyKind(published, DateTimeKind.Utc),
                DocHash: hash,
                Lang: "en"));
        }

        _logger.LogInformation("EDGAR fetched {Count} filings since {SinceUtc:u}", results.Count, sinceUtc);
        return results;
    }

    private static string GuessType(string title, IEnumerable<SyndicationCategory> cats)
    {
        var known = new[] { "10-K", "10-Q", "8-K", "20-F", "6-K" };
        foreach (var k in known)
            if (title.Contains(k, StringComparison.OrdinalIgnoreCase))
                return k;
        var c = cats?.FirstOrDefault()?.Name;
        return string.IsNullOrWhiteSpace(c) ? "Unknown" : c!;
    }
}
