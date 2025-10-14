using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingPlatform.Application.Analyst;
using TradingPlatform.Domain.Analyst;
using TradingPlatform.Domain.Symbols;
using TradingPlatform.Infrastructure.Analyst;
using TradingPlatform.Infrastructure.Symbols;
using Xunit;

namespace TradingPlatform.Tests.Analyst;

/// <summary>Validates that the use-case pulls active symbols and batches into repo.</summary>
public sealed class AnalystIngestionUseCaseTests
{
    [Fact]
    public async Task RunOnceAsync_Pulls_Only_Active_Symbols_And_Upserts()
    {
        // Arrange: 2 active + 1 inactive
        var symbols = new FakeSymbolRegistry(new[]
        {
            new SymbolRow("AAPL", true),
            new SymbolRow("MSFT", true),
            new SymbolRow("ZZZZ", false),
        });

        var source = new FakeAnalystSourceReturningOnePerSymbol();
        var repo = new FakeAnalystRepo();

        var uc = new AnalystIngestionUseCase(symbols, source, repo);

        // Act
        var now = DateTime.UtcNow;
        await uc.RunOnceAsync(now.AddDays(-1), now, CancellationToken.None);

        // Assert: got exactly 2 reports (AAPL, MSFT) and none for ZZZZ
        Assert.Equal(2, repo.LastUpsert.Count);
        Assert.Contains(repo.LastUpsert, r => r.Symbol == "AAPL");
        Assert.Contains(repo.LastUpsert, r => r.Symbol == "MSFT");
        Assert.DoesNotContain(repo.LastUpsert, r => r.Symbol == "ZZZZ");
    }

    // --------- Fakes ---------
    private sealed record SymbolRow(string SymbolCode, bool IsActive);

    private sealed class FakeSymbolRegistry : TradingPlatform.Infrastructure.Symbols.ISymbolRegistry
    {
        private readonly IReadOnlyList<SymbolRow> _rows;
        public FakeSymbolRegistry(IEnumerable<SymbolRow> rows) => _rows = rows.ToList();

        public Task<IReadOnlyList<dynamic>> GetAllAsync(string? exchange, int take, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<dynamic>>(_rows.ToList());

        // Overload often used in project – keep minimal shape to compile if required elsewhere.
        public Task<IReadOnlyList<dynamic>> GetAllAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<dynamic>>(_rows.ToList());

        public Task<IReadOnlyList<Symbol>> SearchAsync(string query, int take, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<string?> TryMatchAsync(string text, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task UpsertAsync(IEnumerable<Symbol> items, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        Task<IReadOnlyList<Symbol>> ISymbolRegistry.GetAllAsync(string? exchange, int take, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeAnalystSourceReturningOnePerSymbol : IAnalystSource
    {
        public Task<IReadOnlyList<AnalystReport>> GetAsync(string symbol, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
        {
            var report = new AnalystReport(
                Symbol: symbol,
                Firm: "Fake",
                Title: "Note",
                Action: "Init",
                RatingFrom: "NA",
                RatingTo: "Buy",
                TargetPrice: 123.45m,
                Currency: "USD",
                Url: $"https://fake/{symbol}",
                Source: "Fake",
                PublishedAt: toUtc.AddMinutes(-5),
                ReportHash: new byte[] { 1, 2, 3, 4, 5 },
                Lang: "en"
            );
            return Task.FromResult<IReadOnlyList<AnalystReport>>(new[] { report });
        }
    }

    private sealed class FakeAnalystRepo : IAnalystReportsRepository
    {
        public List<AnalystReport> LastUpsert { get; } = new();

        public Task<DateTime?> GetLatestDateAsync(string? symbol, CancellationToken ct)
            => Task.FromResult<DateTime?>(null);

        public Task<IReadOnlyList<AnalystReport>> GetBySymbolAsync(string symbol, int take, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<AnalystReport>>(Array.Empty<AnalystReport>());

        public Task UpsertBulkAsync(IEnumerable<AnalystReport> items, CancellationToken ct)
        {
            LastUpsert.AddRange(items);
            return Task.CompletedTask;
        }
    }
}
