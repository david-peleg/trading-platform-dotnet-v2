using System.Data;
using FluentAssertions;
using TradingPlatform.Domain.Prices;
using TradingPlatform.Infrastructure.Prices;
using Xunit;

namespace TradingPlatform.Tests.Prices;

public class PricesTvpTests
{
    [Fact]
    public void Build_CreatesTable_WithCorrectSchema_AndRows()
    {
        var items = new[]
        {
            new PriceDaily("AAPL", new DateOnly(2025, 9, 1), 100,101,99,100.5m, 1_000_000, "Dummy"),
            new PriceDaily("AAPL", new DateOnly(2025, 9, 2), 100.5m,102,100,101.2m, 900_000, "Dummy")
        };

        var dt = PriceTvpBuilder.Build(items);

        dt.Columns.Count.Should().Be(8);
        dt.Columns["Symbol"].DataType.Should().Be(typeof(string));
        dt.Columns["Dt"].DataType.Should().Be(typeof(DateTime));
        dt.Columns["Open"].DataType.Should().Be(typeof(decimal));
        dt.Columns["High"].DataType.Should().Be(typeof(decimal));
        dt.Columns["Low"].DataType.Should().Be(typeof(decimal));
        dt.Columns["Close"].DataType.Should().Be(typeof(decimal));
        dt.Columns["Volume"].DataType.Should().Be(typeof(long));
        dt.Columns["Source"].DataType.Should().Be(typeof(string));
        dt.Rows.Count.Should().Be(2);
    }
}
