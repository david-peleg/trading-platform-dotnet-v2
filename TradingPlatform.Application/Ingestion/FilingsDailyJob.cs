using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TradingPlatform.Domain.Filings;

namespace TradingPlatform.Application.Ingestion;

/// <summary>Daily filings ingestion since last 24h (idempotent by DocHash).</summary>
public sealed class FilingsDailyJob
{
    private readonly IFilingsSource _source;
    private readonly IFilingsRepository _repo;
    private readonly ILogger<FilingsDailyJob> _logger;

    public FilingsDailyJob(IFilingsSource source, IFilingsRepository repo, ILogger<FilingsDailyJob> logger)
    {
        _source = source;
        _repo = repo;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddDays(-1);
        var items = await _source.FetchSinceAsync(since, ct);
        var inserted = await _repo.UpsertAsync(items, ct);
        _logger.LogInformation("FilingsDaily inserted: {Inserted}", inserted);
        return inserted;
    }
}
