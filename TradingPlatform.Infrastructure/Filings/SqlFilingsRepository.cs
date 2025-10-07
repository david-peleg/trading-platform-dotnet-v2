using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;
using Microsoft.Data.SqlClient;
using TradingPlatform.Domain.Filings;

namespace TradingPlatform.Infrastructure.Filings;

/// <summary>Dapper repo writing filings via dbo.SP_Filings_UpsertBulk (TVP).</summary>
public sealed class SqlFilingsRepository : IFilingsRepository
{
    private readonly string _cs;

    public SqlFilingsRepository(string connectionString)
        => _cs = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public async Task<int> UpsertAsync(IEnumerable<Filing> items, CancellationToken ct)
    {
        var dt = new DataTable();
        dt.Columns.Add("Symbol", typeof(string));
        dt.Columns.Add("FilingType", typeof(string));
        dt.Columns.Add("PeriodStart", typeof(DateTime));   // DateOnly? -> DateTime?
        dt.Columns.Add("PeriodEnd", typeof(DateTime));
        dt.Columns.Add("Url", typeof(string));
        dt.Columns.Add("Source", typeof(string));
        dt.Columns.Add("PublishedAt", typeof(DateTime));
        dt.Columns.Add("DocHash", typeof(byte[]));
        dt.Columns.Add("Lang", typeof(string));

        foreach (var f in items)
        {
            dt.Rows.Add(
                f.Symbol,
                f.FilingType,
                f.PeriodStart.HasValue ? f.PeriodStart.Value.ToDateTime(TimeOnly.MinValue) : (object)DBNull.Value,
                f.PeriodEnd.HasValue ? f.PeriodEnd.Value.ToDateTime(TimeOnly.MinValue) : (object)DBNull.Value,
                f.Url,
                f.Source,
                f.PublishedAtUtc,
                f.DocHash,
                (object?)f.Lang ?? DBNull.Value
            );
        }

        await using var con = new SqlConnection(_cs);
        var p = new DynamicParameters();
        p.Add("@Rows", dt.AsTableValuedParameter("dbo.FilingRowType"));

        // Returns single row: { Inserted = int }
        var result = await con.QuerySingleAsync<int>(
            new CommandDefinition(
                commandText: "dbo.SP_Filings_UpsertBulk",
                parameters: p,
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

        return result;
    }
}
