using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using TradingPlatform.Domain.News;

namespace TradingPlatform.Infrastructure.News;

/// <summary>
/// Dapper implementation that calls approved SPs only.
/// </summary>
public sealed class SqlNewsRepository : INewsRepository
{
    private readonly string _cs;
    public SqlNewsRepository(string connectionString) => _cs = connectionString;

    public async Task UpsertBulkAsync(IEnumerable<RawNews> items, CancellationToken ct)
    {
        using var con = new SqlConnection(_cs);
        using var cmd = new SqlCommand("dbo.RawNews_UpsertBulk", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        var tvp = new DataTable();
        tvp.Columns.Add("Ticker", typeof(string));
        tvp.Columns.Add("Headline", typeof(string));
        tvp.Columns.Add("Url", typeof(string));
        tvp.Columns.Add("Source", typeof(string));
        tvp.Columns.Add("PublishedAt", typeof(DateTime));
        tvp.Columns.Add("BodyHash", typeof(byte[]));
        tvp.Columns.Add("Lang", typeof(string));

        foreach (var n in items)
            tvp.Rows.Add(n.Ticker, n.Headline, n.Url, n.Source, n.PublishedAtUtc, n.BodyHash, n.Lang);

        var p = cmd.Parameters.AddWithValue("@Rows", tvp);
        p.SqlDbType = SqlDbType.Structured;
        p.TypeName = "dbo.RawNewsType";

        await con.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<DateTime?> GetLatestDateAsync(CancellationToken ct)
    {
        using var con = new SqlConnection(_cs);
        var r = await con.QueryFirstOrDefaultAsync<DateTime?>(
            new CommandDefinition("dbo.RawNews_GetLatestDate", commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return r;
    }

    public async Task<IReadOnlyList<RawNews>> GetLatestAsync(string? symbol, int take, CancellationToken ct)
    {
        using var con = new SqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("@Symbol", symbol, DbType.String);
        p.Add("@Take", take, DbType.Int32);
        var rows = await con.QueryAsync(
            new CommandDefinition("dbo.RawNews_GetLatest", p, commandType: CommandType.StoredProcedure, cancellationToken: ct));

        var list = new List<RawNews>();
        foreach (var r in rows)
        {
            list.Add(new RawNews(
                (string?)r.Ticker,
                (string)r.Headline,
                (string)r.Url,
                (string)r.Source,
                (DateTime)r.PublishedAt,
                (byte[])r.BodyHash,
                (string?)r.Lang));
        }
        return list;
    }
}
