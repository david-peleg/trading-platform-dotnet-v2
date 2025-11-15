using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using TradingPlatform.Domain.Features;
using TradingPlatform.Infrastructure.Common;

namespace TradingPlatform.Infrastructure.Features;

/// <summary>
/// SQL Server implementation of IFeatureRepository using SP-only access.
/// </summary>
public sealed class SqlFeatureRepository : IFeatureRepository
{
    private const string SpUpsertBulk = "dbo.Features_UpsertBulk";
    private const string SpDeleteRange = "dbo.Features_DeleteRange";
    private const string SpGetBySymbol = "dbo.Features_GetBySymbol";

    private readonly IDb _db;

    public SqlFeatureRepository(IDb db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task UpsertBulkAsync(IEnumerable<Feature> items, CancellationToken ct)
    {
        var table = BuildFeatureTable(items);

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = SpUpsertBulk;
        command.CommandType = CommandType.StoredProcedure;

        var tvp = new SqlParameter("@Rows", SqlDbType.Structured)
        {
            TypeName = "dbo.FeatureType",
            Value = table
        };

        command.Parameters.Add(tvp);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteRangeAsync(string symbol, DateOnly from, DateOnly to, string? prefix, CancellationToken ct)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.ToDateTime(TimeOnly.MinValue);

        await _db.ExecuteAsync(
            SpDeleteRange,
            new
            {
                Symbol = symbol,
                From = fromDateTime,
                To = toDateTime,
                Prefix = prefix
            },
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Feature>> GetBySymbolAsync(string symbol, DateOnly from, DateOnly to, string? prefix, CancellationToken ct)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.ToDateTime(TimeOnly.MinValue);

        var rows = await _db.QueryAsync<FeatureRow>(
            SpGetBySymbol,
            new
            {
                Symbol = symbol,
                From = fromDateTime,
                To = toDateTime,
                Prefix = prefix
            },
            ct).ConfigureAwait(false);

        var features = rows
            .Select(r => new Feature(
                r.Symbol,
                DateOnly.FromDateTime(r.Dt),
                r.FeatureName,
                r.Value,
                r.Window))
            .ToList();

        return features;
    }

    private static DataTable BuildFeatureTable(IEnumerable<Feature> items)
    {
        var table = new DataTable
        {
            TableName = "FeatureType"
        };

        table.Columns.Add("Symbol", typeof(string));
        table.Columns.Add("Dt", typeof(DateTime));
        table.Columns.Add("FeatureName", typeof(string));
        table.Columns.Add("Value", typeof(double));
        table.Columns.Add("Window", typeof(short));

        foreach (var item in items)
        {
            var row = table.NewRow();
            row["Symbol"] = item.Symbol;
            row["Dt"] = item.Dt.ToDateTime(TimeOnly.MinValue);
            row["FeatureName"] = item.FeatureName;
            row["Value"] = item.Value;
            row["Window"] = item.Window.HasValue ? item.Window.Value : DBNull.Value;
            table.Rows.Add(row);
        }

        return table;
    }

    private sealed class FeatureRow
    {
        public string Symbol { get; init; } = string.Empty;
        public DateTime Dt { get; init; }
        public string FeatureName { get; init; } = string.Empty;
        public double Value { get; init; }
        public short? Window { get; init; }
    }
}
