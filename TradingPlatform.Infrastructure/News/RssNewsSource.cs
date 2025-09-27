using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using TradingPlatform.Domain.Ingestion;
using TradingPlatform.Domain.News;

namespace TradingPlatform.Infrastructure.News;

/// <summary>
/// Reads RSS feeds via HttpClient + SyndicationFeed; computes hash & basic lang detection.
/// </summary>
public sealed class RssNewsSource : INewsSource
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RssNewsSource> _logger;
    private readonly string[] _feeds;
    private readonly AsyncRetryPolicy _retry;

    public RssNewsSource(
        IHttpClientFactory httpClientFactory,
        IOptions<IngestionOptions> options,
        ILogger<RssNewsSource> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _feeds = options.Value.Feeds ?? Array.Empty<string>();
        _retry = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(1 * i));
    }

    /// <summary>Stream items across all configured feeds within the given UTC window.</summary>
    public async IAsyncEnumerable<RawNews> ReadAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(nameof(RssNewsSource));
        foreach (var feedUrl in _feeds)
        {
            SyndicationFeed? feed = null;
            // RssNewsSource.cs (החלף את הבלוק של GetAsync + EnsureSuccessStatusCode)
            try
            {
                await _retry.ExecuteAsync(async () =>
                    {
                        using var resp = await client.GetAsync(feedUrl, ct);

                        // אם זה 404/410/403 — לא זורקים חריג; רק לוג וממשיכים לפיד הבא
                        if ((int)resp.StatusCode == 404 || (int)resp.StatusCode == 410 || (int)resp.StatusCode == 403)
                        {
                            _logger.LogWarning("RSS feed returned {Status} for {FeedUrl}. Skipping.", (int)resp.StatusCode, feedUrl);
                            feed = null; // ישאיר את הפיד כ-null ונדלג
                            return;
                        }

                        // סטטוסים אחרים: אם נכשל — Polly תנסה שוב; אם ימשיך להיכשל, נתפוס בהמשך ולא נפיל Host
                        resp.EnsureSuccessStatusCode();

                        await using var s = await resp.Content.ReadAsStreamAsync(ct);
                        using var xr = XmlReader.Create(s, new XmlReaderSettings { Async = true });
                        feed = SyndicationFeed.Load(xr);
                    });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed fetching feed {FeedUrl}", feedUrl);
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error on feed {FeedUrl}", feedUrl);
                continue;
            }

            if (feed == null) continue; // דילוג נקי על הפיד הבעייתי

            foreach (var i in feed.Items)
            {
                var published = i.PublishDate.UtcDateTime;
                if (published < fromUtc || published > toUtc) continue;

                var title = i.Title?.Text?.Trim() ?? "";
                var link = i.Links.FirstOrDefault()?.Uri?.ToString() ?? "";
                var source = feed.Title?.Text?.Trim() ?? "rss";
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link)) continue;

                var hash = NewsHash.Compute(title, link, source, published);
                var lang = ContainsHebrew(title) ? "he" : "en";

                yield return new RawNews(
                    Ticker: null,
                    Headline: title,
                    Url: link,
                    Source: source,
                    PublishedAtUtc: published,
                    BodyHash: hash,
                    Lang: lang);
            }
        }
    }

    /// <summary>Very light heuristic: if Hebrew letters exist → "he".</summary>
    private static bool ContainsHebrew(string s) => s.Any(ch => ch >= 0x0590 && ch <= 0x05FF);
}
