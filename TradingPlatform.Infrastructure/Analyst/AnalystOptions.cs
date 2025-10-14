// src/Infrastructure/Analyst/AnalystOptions.cs
namespace TradingPlatform.Infrastructure.Analyst;

/// <summary>Configuration for analyst ingestion source selection and HTTP/FMP settings.</summary>
public sealed class AnalystOptions
{
    public const string SectionName = "Analyst";
    public string Source { get; set; } = "Dummy";
    public HttpOptions Http { get; set; } = new();
    public FmpOptions Fmp { get; set; } = new();

    public sealed class HttpOptions
    {
        public string BaseUrl { get; set; } = "";
        public string ApiKeyHeader { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string Lang { get; set; } = "en";
        public string SourceName { get; set; } = "Http";
    }

    public sealed class FmpOptions
    {
        public string BaseUrl { get; set; } = "https://financialmodelingprep.com/api/v4";
        public string ApiKey { get; set; } = "";
        public string Lang { get; set; } = "en";
        public string SourceName { get; set; } = "FMP";
    }
}
