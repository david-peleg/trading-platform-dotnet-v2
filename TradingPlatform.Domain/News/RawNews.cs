namespace TradingPlatform.Domain.News;

/// <summary>
/// Raw news item as ingested from external feeds; stored as-is for later attribution/analysis.
/// </summary>
public sealed record RawNews(
    string? Ticker,
    string Headline,
    string Url,
    string Source,
    DateTime PublishedAtUtc,
    byte[] BodyHash,
    string? Lang);
