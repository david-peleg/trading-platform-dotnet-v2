// src/Infrastructure/Analyst/SqlAnalystReportsRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using TradingPlatform.Domain.Analyst;
using TradingPlatform.Infrastructure.Common;

namespace TradingPlatform.Infrastructure.Analyst;

/// <summary>Dapper-based repository that calls stored procedures via IDb helper (SP-only).</summary>
public sealed class SqlAnalystReportsRepository : IAnalystReportsRepository
{
    private readonly IDb _db;

    public SqlAnalystReportsRepository(IDb db) => _db = db;

    /// <summary>Bulk upsert using TVP (dbo.AnalystReportRow) via dbo.AnalystReports_UpsertBulk.</summary>
    public async Task UpsertBulkAsync(IEnumerable<AnalystReport> items, CancellationToken ct)
    {
        var dt = new DataTable();
        dt.Columns.Add("Symbol", typeof(string));
        dt.Columns.Add("Firm", typeof(string));
        dt.Columns.Add("Title", typeof(string));
        dt.Columns.Add("Action", typeof(string));
        dt.Columns.Add("RatingFrom", typeof(string));
        dt.Columns.Add("RatingTo", typeof(string));
        dt.Columns.Add("TargetPrice", typeof(decimal));
        dt.Columns.Add("Currency", typeof(string));
        dt.Columns.Add("Url", typeof(string));
        dt.Columns.Add("Source", typeof(string));
        dt.Columns.Add("PublishedAt", typeof(DateTime));
        dt.Columns.Add("ReportHash", typeof(byte[]));
        dt.Columns.Add("Lang", typeof(string));

        foreach (var x in items)
        {
            dt.Rows.Add(
                x.Symbol, x.Firm, x.Title, x.Action, x.RatingFrom, x.RatingTo,
                (object?)x.TargetPrice ?? DBNull.Value, x.Currency, x.Url, x.Source,
                x.PublishedAt, x.ReportHash, x.Lang
            );
        }

        await using var con = _db.CreateConnection();
        await con.OpenAsync(ct).ConfigureAwait(false);

        await using var cmd = new SqlCommand("dbo.AnalystReports_UpsertBulk", con)
        { CommandType = CommandType.StoredProcedure };

        var tvp = cmd.Parameters.AddWithValue("@Rows", dt);
        tvp.SqlDbType = SqlDbType.Structured;
        tvp.TypeName = "dbo.AnalystReportType";

        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    /// <summary>Returns MAX(PublishedAt) for a symbol (or overall if null) via dbo.AnalystReports_GetLatestDate.</summary>
    public async Task<DateTime?> GetLatestDateAsync(string? symbol, CancellationToken ct)
    {
        var rows = await _db.QueryAsync<DateTime?>(
            "dbo.AnalystReports_GetLatestDate",
            new { Symbol = symbol },
            ct);

        // the SP returns a single scalar row
        return rows.FirstOrDefault();
    }

    /// <summary>Gets latest N reports for a symbol via dbo.AnalystReports_GetBySymbol.</summary>
    public async Task<IReadOnlyList<AnalystReport>> GetBySymbolAsync(string symbol, int take, CancellationToken ct)
    {
        var rows = await _db.QueryAsync<Row>(
            "dbo.AnalystReports_GetBySymbol",
            new { Symbol = symbol, Take = take },
            ct);

        var list = rows.Select(r => new AnalystReport(
            r.Symbol, r.Firm, r.Title, r.Action, r.RatingFrom, r.RatingTo,
            r.TargetPrice, r.Currency, r.Url, r.Source, r.PublishedAt, r.ReportHash, r.Lang
        )).ToList();

        return list;
    }

    private sealed class Row
    {
        public string Symbol { get; set; } = "";
        public string Firm { get; set; } = "";
        public string? Title { get; set; }
        public string? Action { get; set; }
        public string? RatingFrom { get; set; }
        public string? RatingTo { get; set; }
        public decimal? TargetPrice { get; set; }
        public string? Currency { get; set; }
        public string Url { get; set; } = "";
        public string Source { get; set; } = "";
        public DateTime PublishedAt { get; set; }
        public byte[] ReportHash { get; set; } = Array.Empty<byte>();
        public string? Lang { get; set; }
    }
}
