using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TradingPlatform.Domain.Filings;

/// <summary>Abstract source for fetching filings (e.g., EDGAR/MAYA). Dev may use Dummy.</summary>
public interface IFilingsSource
{
    /// <summary>Fetch filings for the last 'daysBack' days.</summary>
    Task<IReadOnlyList<Filing>> FetchAsync(int daysBack, CancellationToken ct);

    /// <summary>Fetch filings published since (UTC).</summary>
    Task<IReadOnlyList<Filing>> FetchSinceAsync(DateTime sinceUtc, CancellationToken ct);
}
