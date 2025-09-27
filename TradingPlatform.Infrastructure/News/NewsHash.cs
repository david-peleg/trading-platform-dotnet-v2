using System.Security.Cryptography;
using System.Text;

namespace TradingPlatform.Infrastructure.News;

/// <summary>
/// Deterministic SHA256 hash helper for news identity.
/// </summary>
public static class NewsHash
{
    /// <summary>Compute SHA256 of "Headline|Url|Source|Ticks".</summary>
    public static byte[] Compute(string headline, string url, string source, DateTime publishedUtc)
    {
        var s = $"{headline}|{url}|{source}|{publishedUtc.Ticks}";
        return SHA256.HashData(Encoding.UTF8.GetBytes(s));
    }
}
