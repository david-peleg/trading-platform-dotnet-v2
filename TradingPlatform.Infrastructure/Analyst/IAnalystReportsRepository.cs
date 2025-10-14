using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradingPlatform.Domain.Analyst;

namespace TradingPlatform.Infrastructure.Analyst;

/// <summary>Repository abstraction for AnalystReports via stored procedures.</summary>
public interface IAnalystReportsRepository
{
    Task UpsertBulkAsync(IEnumerable<AnalystReport> items, CancellationToken ct);
    Task<DateTime?> GetLatestDateAsync(string? symbol, CancellationToken ct);
    Task<IReadOnlyList<AnalystReport>> GetBySymbolAsync(string symbol, int take, CancellationToken ct);
}
