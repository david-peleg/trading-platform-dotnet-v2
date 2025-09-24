using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using TradingPlatform.Domain.Prices;
using TradingPlatform.Infrastructure.Common;
using TradingPlatform.Infrastructure.Prices;
using Xunit;

namespace TradingPlatform.Tests.Prices;

public class PricesRepositoryContractTests
{
    private sealed class SpyDb : IDb
    {
        public string? LastProc { get; private set; }
        public object? LastParam { get; private set; }

        public Task<int> ExecuteAsync(string spName, object? parameters, CancellationToken ct)
        { LastProc = spName; LastParam = parameters; return Task.FromResult(0); }

        public Task<IEnumerable<T>> QueryAsync<T>(string spName, object? parameters, CancellationToken ct)
        { LastProc = spName; LastParam = parameters; return Task.FromResult<IEnumerable<T>>(Array.Empty<T>()); }

        public SqlConnection CreateConnection() => new SqlConnection("Server=.;Database=master;Trusted_Connection=True;");
    }

    [Fact]
    public async Task UpsertDailyAsync_Calls_Correct_SP_Name()
    {
        var spy = new SpyDb();
        var repo = new SqlPriceRepository(spy);
        var rows = new[] { new PriceDaily("AAPL", new DateOnly(2025, 9, 1), 1, 2, 0.5m, 1.5m, 123, "Dummy") };

        var tvp = PriceTvpBuilder.Build(rows);
        tvp.Rows.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetLatestDateAsync_Calls_Correct_SP_Name()
    {
        var spy = new SpyDb();
        var repo = new SqlPriceRepository(spy);
        _ = await repo.GetLatestDateAsync("AAPL", default);
        spy.LastProc.Should().Be("dbo.Prices_GetLatestDate");
    }

    [Fact]
    public async Task GetSeriesAsync_Calls_Correct_SP_Name()
    {
        var spy = new SpyDb();
        var repo = new SqlPriceRepository(spy);
        _ = await repo.GetSeriesAsync("AAPL", new DateOnly(2025, 9, 1), new DateOnly(2025, 9, 10), default);
        spy.LastProc.Should().Be("dbo.Prices_GetSeries");
    }
}
