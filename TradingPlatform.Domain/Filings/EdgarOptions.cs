namespace TradingPlatform.Domain.Filings;

/// <summary>Options for EDGAR RSS source (bound to "Filings:Edgar").</summary>
public sealed class EdgarOptions
{
    public const string SectionName = "Filings:Edgar";
    public string FeedUrl { get; init; } =
        "https://www.sec.gov/cgi-bin/browse-edgar?action=getcurrent&owner=include&output=atom";
    public string UserAgent { get; init; } =
        "trading-platform-net8/0.1 (contact: your-email@your-domain.com)";
}
