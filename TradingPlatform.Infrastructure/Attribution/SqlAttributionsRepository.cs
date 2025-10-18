using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using TradingPlatform.Domain.Attribution;
using TradingPlatform.Infrastructure.Common;
// alias לטיפוס הדומיין
using DomainAttribution = TradingPlatform.Domain.Attribution.Attribution;

namespace TradingPlatform.Infrastructure.Attributions;

/// <summary>SP-only repository for Attributions (uses IDb; no inline SQL).</summary>
public sealed class SqlAttributionsRepository : IAttributionsRepository
{
    private readonly IDb _db;
    private readonly ILogger<SqlAttributionsRepository> _log;

    public SqlAttributionsRepository(IDb db, ILogger<SqlAttributionsRepository> log)
    {
        _db = db;
        _log = log;
    }

    public async Task InsertBulkAsync(IReadOnlyList<DomainAttribution> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return;

        var tvp = new DataTable();
        tvp.Columns.Add("ItemType", typeof(byte));
        tvp.Columns.Add("ItemId", typeof(long));
        tvp.Columns.Add("Symbol", typeof(string));
        tvp.Columns.Add("Direction", typeof(short));
        tvp.Columns.Add("Confidence", typeof(double));
        tvp.Columns.Add("HorizonD", typeof(short));

        foreach (var r in rows)
            tvp.Rows.Add((byte)r.ItemType, r.ItemId, r.Symbol, r.Direction, r.Confidence, r.HorizonD);

        var rowsParam = new SqlParameter("@Rows", tvp)
        {
            SqlDbType = SqlDbType.Structured,
            TypeName = "dbo.AttributionRow"
        };

        await _db.ExecuteAsync("dbo.Attributions_InsertBulk", new { Rows = rowsParam }, ct);
    }

    public async Task<IReadOnlyList<DomainAttribution>> GetBySymbolAsync(string symbol, short? horizonD, int take, CancellationToken ct)
    {
        var data = await _db.QueryAsync<Row>(
            "dbo.Attributions_GetBySymbol",
            new { Symbol = symbol, HorizonD = horizonD, Take = take },
            ct);

        return data.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<DomainAttribution>> GetForItemAsync(ItemType itemType, long itemId, CancellationToken ct)
    {
        var data = await _db.QueryAsync<Row>(
            "dbo.Attributions_GetForItem",
            new { ItemType = (byte)itemType, ItemId = itemId },
            ct);

        return data.Select(Map).ToList();
    }

    private static DomainAttribution Map(Row r) =>
        new((ItemType)r.ItemType, r.ItemId, r.Symbol, r.Direction, r.Confidence, r.HorizonD);

    private sealed class Row
    {
        public byte ItemType { get; init; }
        public long ItemId { get; init; }
        public string Symbol { get; init; } = "";
        public short Direction { get; init; }
        public double Confidence { get; init; }
        public short HorizonD { get; init; }
    }
}
