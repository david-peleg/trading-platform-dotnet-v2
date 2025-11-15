using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using TradingPlatform.Domain.Features;
using TradingPlatform.Infrastructure.Common;
using TradingPlatform.Infrastructure.Features;
using Xunit;

namespace TradingPlatform.Tests.Features;

/// <summary>
/// Basic contract tests for SqlFeatureRepository SP usage.
/// </summary>
public sealed class FeatureRepositoryContractTests
{
    [Fact]
    public async Task DeleteRange_UsesExpectedStoredProcedureName()
    {
        var fakeDb = new FakeDb();
        var repo = new SqlFeatureRepository(fakeDb);

        await repo.DeleteRangeAsync("AAPL", new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31), "MA_", CancellationToken.None);

        fakeDb.LastSpName.Should().Be("dbo.Features_DeleteRange");
    }

    [Fact]
    public async Task GetBySymbol_UsesExpectedStoredProcedureName()
    {
        var fakeDb = new FakeDb();
        var repo = new SqlFeatureRepository(fakeDb);

        await repo.GetBySymbolAsync("AAPL", new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31), null, CancellationToken.None);

        fakeDb.LastSpName.Should().Be("dbo.Features_GetBySymbol");
    }

    private sealed class FakeDb : IDb
    {
        public string? LastSpName { get; private set; }

        public Task<int> ExecuteAsync(string spName, object? parameters, CancellationToken ct)
        {
            LastSpName = spName;
            return Task.FromResult(0);
        }

        public Task<IEnumerable<T>> QueryAsync<T>(string spName, object? parameters, CancellationToken ct)
        {
            LastSpName = spName;
            return Task.FromResult<IEnumerable<T>>(Array.Empty<T>());
        }

        public SqlConnection CreateConnection()
        {
            // Not used in these tests.
            throw new NotSupportedException("CreateConnection is not used in FakeDb tests.");
        }
    }
}
