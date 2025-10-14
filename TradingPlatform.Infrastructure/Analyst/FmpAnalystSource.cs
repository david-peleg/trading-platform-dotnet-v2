// src/Infrastructure/Analyst/FmpAnalystSource.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TradingPlatform.Domain.Analyst;

namespace TradingPlatform.Infrastructure.Analyst;

/// <summary>
/// Analyst source via Financial Modeling Prep (Upgrades/Downgrades).
/// Maps: Firm, Action, FromGrade→RatingFrom, ToGrade→RatingTo, Url, PublishedAt. TargetPrice/Currency usually null.
/// </summary>
public sealed class FmpAnalystSource : IAnalystSource
{
    private readonly IHttpClientFactory _http;
    private readonly AnalystOptions _opt;
    private static readonly JsonSerializerOptions J = new() { PropertyNameCaseInsensitive = true };

    public FmpAnalystSource(IHttpClientFactory http, IOptions<AnalystOptions> opt)
        => (_http, _opt) = (http, opt.Value);

    public async Task<IReadOnlyList<AnalystReport>> GetAsync(string symbol, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        // https://financialmodelingprep.com/api/v4/upgrades-downgrades?symbol=AAPL&from=2025-01-01&to=2025-10-14&apikey=KEY
        var from = fromUtc.ToString("yyyy-MM-dd");
        var to = toUtc.ToString("yyyy-MM-dd");
        var url = $"{_opt.Fmp.BaseUrl.TrimEnd('/')}/upgrades-downgrades?symbol={Uri.EscapeDataString(symbol)}&from={from}&to={to}&apikey={Uri.EscapeDataString(_opt.Fmp.ApiKey)}";

        var client = _http.CreateClient(nameof(FmpAnalystSource));
        using var resp = await client.GetAsync(url, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var rows = await JsonSerializer.DeserializeAsync<List<Row>>(stream, J, ct).ConfigureAwait(false) ?? new();

        var list = new List<AnalystReport>(rows.Count);
        foreach (var r in rows)
        {
            var firm = r.AnalystCompany ?? r.Company ?? "Unknown";
            var title = BuildTitle(r.Action, r.ToGrade, r.FromGrade);
            var reportUrl = r.NewsURL ?? r.NewsUrl ?? r.Url ?? string.Empty;
            var published = (r.PublishedDate ?? r.Date)?.UtcDateTime ?? toUtc;

            // Hash: Symbol|Firm|Title|PublishedAt|Url
            var idForHash = $"{symbol}|{firm}|{title}|{published:O}|{reportUrl}";
            var hash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(idForHash));

            // NOTE: AnalystReport has 13 params (no Id/CreatedAt). TargetPrice/Currency -> null for FMP UD feed.
            list.Add(new AnalystReport(
                Symbol: symbol,
                Firm: firm,
                Title: title,
                Action: r.Action,
                RatingFrom: r.FromGrade,
                RatingTo: r.ToGrade,
                TargetPrice: null,
                Currency: null,
                Url: reportUrl,
                Source: _opt.Fmp.SourceName,
                PublishedAt: published,
                ReportHash: hash,
                Lang: _opt.Fmp.Lang
            ));
        }
        return list;
    }

    private static string? BuildTitle(string? action, string? to, string? from)
        => action is null && to is null ? null
           : from is null ? $"{action} {to}".Trim()
           : $"{action} {to} (from {from})".Trim();

    private sealed class Row
    {
        public string? AnalystCompany { get; set; }
        public string? Company { get; set; }
        public string? Action { get; set; }
        public string? FromGrade { get; set; }
        public string? ToGrade { get; set; }
        public string? Url { get; set; }
        public string? NewsUrl { get; set; }
        public string? NewsURL { get; set; }
        public DateTimeOffset? Date { get; set; }
        public DateTimeOffset? PublishedDate { get; set; }
    }
}
