using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TradingPlatform.Domain.Filings;

namespace TradingPlatform.Application.Ingestion;

/// <summary>Backfills filings for the last N days via source → repo (SP-Only).</summary>
public sealed class FilingsBackfillJob
{
    private readonly IFilingsSource _source;
    private readonly IFilingsRepository _repo;
    private readonly ILogger<FilingsBackfillJob> _logger;

    public FilingsBackfillJob(IFilingsSource source, IFilingsRepository repo, ILogger<FilingsBackfillJob> logger)
    {
        _source = source;
        _repo = repo;
        _logger = logger;
    }

    public async Task<int> RunAsync(int daysBack, CancellationToken ct)
    {
        var items = await _source.FetchAsync(daysBack, ct);
        var inserted = await _repo.UpsertAsync(items, ct);
        _logger.LogInformation("FilingsBackfill inserted: {Inserted}", inserted);
        return inserted;
    }
}
