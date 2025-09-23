// tests/TradingPlatform.Tests/Symbols/SymbolsTvPTests.cs
using System.Data;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using TradingPlatform.Domain.Symbols;
using TradingPlatform.Infrastructure.Symbols;
using Xunit;

namespace TradingPlatform.Tests.Symbols;

public class SymbolsTvPTests
{
    [Fact]
    public void BuildSymbolsTvp_BuildsCorrectSchema_AndRows()
    {
        var items = new[]
        {
            new Symbol("AAPL", "Apple", "NASDAQ", "US", "Tech", true),
            new Symbol("TEVA", "Teva", "TASE", "IL", "Health", true)
        };

        var dt = SqlSymbolRegistry.BuildSymbolsTvp(items);

        dt.Columns.Count.Should().Be(6);
        dt.Columns["Symbol"].DataType.Should().Be(typeof(string));
        dt.Columns["IsActive"].DataType.Should().Be(typeof(bool));

        dt.Rows.Count.Should().Be(2);
        dt.Rows[0]["Symbol"].Should().Be("AAPL");
        dt.Rows[1]["Exchange"].Should().Be("TASE");

        var p = new SqlParameter("@Rows", SqlDbType.Structured)
        {
            TypeName = "dbo.SymbolType",
            Value = dt
        };

        p.SqlDbType.Should().Be(SqlDbType.Structured);
        p.TypeName.Should().Be("dbo.SymbolType");
        p.Value.Should().BeSameAs(dt);
    }
}
