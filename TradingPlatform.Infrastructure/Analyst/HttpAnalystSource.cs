using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TradingPlatform.Domain.Analyst;

namespace TradingPlatform.Infrastructure.Analyst;

/// <summary>
/// HTTP-based analyst source. Calls a provider endpoint returning JSON rows and maps to AnalystReport.
/// Expected JSON array fields per item: firm, title, action, ratingFrom, ratingTo, targetPrice, currency, url, publishedAt (ISO).
/// </summary>
public sealed class HttpAnalystSource : IAnalystSource
{
    private readonly IHttpClientFactory _http;
    private readonly AnalystOptions _opt;
    private static readonly JsonSerializerOptions JsonOpt = new() { PropertyNameCaseInsensitive = true };

    public HttpAnalystSource(IHttpClientFactory http, IOptions<AnalystOptions> opt)
        => (_http, _opt) = (http, opt.Value);

    public async Task<IReadOnlyList<AnalystReport>> GetAsync(string symbol, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        var client = _http.CreateClient(nameof(HttpAnalystSource));
        if (!string.IsNullOrWhiteSpace(_opt.Http.ApiKeyHeader) && !string.IsNullOrWhiteSpace(_opt.Http.ApiKey))
            client.DefaultRequestHeaders.TryAddWithoutValidation(_opt.Http.ApiKeyHeader, _opt.Http.ApiKey);

        var url = $"{_opt.Http.BaseUrl.TrimEnd('/')}/analyst?symbol={Uri.EscapeDataString(symbol)}&from={Uri.EscapeDataString(fromUtc.ToString("O"))}&to={Uri.EscapeDataString(toUtc.ToString("O"))}";
        using var resp = await client.GetAsync(url, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var rows = await JsonSerializer.DeserializeAsync<List<Row>>(stream, JsonOpt, ct).ConfigureAwait(false) ?? new();

        var list = new List<AnalystReport>(rows.Count);
        foreach (var r in rows)
        {
            DateTime published = r.PublishedAt?.UtcDateTime ?? toUtc;
            var title = r.Title ?? "";
            var firm = r.Firm ?? "";
            var reportUrl = r.Url ?? "";

            var hash = ComputeHash($"{symbol}|{firm}|{title}|{published:O}|{reportUrl}");
            list.Add(new AnalystReport(
                symbol,
                firm,
                r.Title,
                r.Action,
                r.RatingFrom,
                r.RatingTo,
                r.TargetPrice,
                r.Currency,
                reportUrl,
                _opt.Http.SourceName,
                published,
                hash,
                _opt.Http.Lang
            ));
        }
        return list;
    }

    private static byte[] ComputeHash(string input)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(input));
    }

    private sealed class Row
    {
        public string? Firm { get; set; }
        public string? Title { get; set; }
        public string? Action { get; set; }
        public string? RatingFrom { get; set; }
        public string? RatingTo { get; set; }
        public decimal? TargetPrice { get; set; }
        public string? Currency { get; set; }
        public string? Url { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
    }
}
