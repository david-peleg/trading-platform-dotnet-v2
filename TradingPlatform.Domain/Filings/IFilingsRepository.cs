using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TradingPlatform.Domain.Filings;

/// <summary>Persists filings via Stored Procedures (no inline SQL).</summary>
public interface IFilingsRepository
{
    /// <summary>Bulk upsert filings using TVP to SP (dedupe by DocHash).</summary>
    Task<int> UpsertAsync(IEnumerable<Filing> items, CancellationToken ct);
}
