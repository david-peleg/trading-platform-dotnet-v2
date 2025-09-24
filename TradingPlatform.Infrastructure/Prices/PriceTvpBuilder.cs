using System.Data;
using TradingPlatform.Domain.Prices;

namespace TradingPlatform.Infrastructure.Prices;

/// <summary>Builds TVP for dbo.PriceDailyType.</summary>
public static class PriceTvpBuilder
{
    public static DataTable Build(IEnumerable<PriceDaily> items)
    {
        var dt = new DataTable();
        dt.Columns.Add("Symbol", typeof(string));
        dt.Columns.Add("Dt", typeof(DateTime));
        dt.Columns.Add("Open", typeof(decimal));
        dt.Columns.Add("High", typeof(decimal));
        dt.Columns.Add("Low", typeof(decimal));
        dt.Columns.Add("Close", typeof(decimal));
        dt.Columns.Add("Volume", typeof(long));
        dt.Columns.Add("Source", typeof(string));

        foreach (var x in items)
        {
            dt.Rows.Add(
                x.Symbol,
                x.Dt.ToDateTime(TimeOnly.MinValue),
                (object?)x.Open ?? DBNull.Value,
                (object?)x.High ?? DBNull.Value,
                (object?)x.Low ?? DBNull.Value,
                (object?)x.Close ?? DBNull.Value,
                (object?)x.Volume ?? DBNull.Value,
                (object?)x.Source ?? DBNull.Value
            );
        }
        return dt;
    }
}
