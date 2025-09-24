using System.Data;
using Dapper;
using TradingPlatform.Domain.Prices;
using TradingPlatform.Infrastructure.Common;

namespace TradingPlatform.Infrastructure.Prices;

public sealed class SqlPriceRepository : IPriceRepository
{
    private readonly IDb _db;
    private const string SP_UPSERT = "dbo.Prices_UpsertDaily";
    private const string SP_LATEST = "dbo.Prices_GetLatestDate";
    private const string SP_SERIES = "dbo.Prices_GetSeries";

    public SqlPriceRepository(IDb db) => _db = db;

    public async Task UpsertDailyAsync(IEnumerable<PriceDaily> items, CancellationToken ct)
    {
        var tvp = PriceTvpBuilder.Build(items);
        await using var conn = _db.CreateConnection();
        var p = new DynamicParameters();
        p.Add("Rows", tvp.AsTableValuedParameter("dbo.PriceDailyType"));
        await conn.ExecuteAsync(new CommandDefinition(SP_UPSERT, p,
            commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<DateOnly?> GetLatestDateAsync(string symbol, CancellationToken ct)
    {
        var rows = await _db.QueryAsync<DateTime?>(SP_LATEST, new { Symbol = symbol }, ct);
        var dt = rows.FirstOrDefault();
        return dt.HasValue ? DateOnly.FromDateTime(dt.Value.Date) : null;
    }

    private sealed record PriceRow(
        string Symbol, DateTime Dt,
        decimal? Open, decimal? High, decimal? Low, decimal? Close,
        long? Volume, string? Source);

    public async Task<IReadOnlyList<PriceDaily>> GetSeriesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var rows = await _db.QueryAsync<PriceRow>(SP_SERIES, new
        {
            Symbol = symbol,
            From = from.ToDateTime(TimeOnly.MinValue),
            To = to.ToDateTime(TimeOnly.MinValue)
        }, ct);

        return rows.Select(r => new PriceDaily(
            r.Symbol,
            DateOnly.FromDateTime(r.Dt.Date),
            r.Open, r.High, r.Low, r.Close,
            r.Volume, r.Source)).ToList();
    }
}
