using Microsoft.Extensions.Logging;
using TradingPlatform.Domain.News;
using TradingPlatform.Infrastructure.News;
using TradingPlatform.Infrastructure.Symbols; // Assumes Prompt 1 provided ISymbolRegistry

namespace TradingPlatform.Application.News;

/// <summary>
/// Runs one ingestion pass: read → match symbol → bulk upsert.
/// </summary>
public sealed class NewsIngestionUseCase
{
    private readonly INewsSource _source;
    private readonly INewsRepository _repo;
    private readonly ISymbolRegistry _symbols;
    private readonly ILogger<NewsIngestionUseCase> _logger;

    public NewsIngestionUseCase(INewsSource source, INewsRepository repo, ISymbolRegistry symbols, ILogger<NewsIngestionUseCase> logger)
        => (_source, _repo, _symbols, _logger) = (source, repo, symbols, logger);

    /// <summary>Ingests news for the time window [fromUtc,toUtc] and stores to DB via SPs.</summary>
    public async Task RunOnceAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        var batch = new List<RawNews>(256);

        await foreach (var n in _source.ReadAsync(fromUtc, toUtc, ct).WithCancellation(ct))
        {
            // Basic symbol match: symbol in parentheses or name occurrence
            var sym = await _symbols.TryMatchAsync(n.Headline, ct);
            batch.Add(n with { Ticker = sym });
            if (batch.Count >= 500)
            {
                await _repo.UpsertBulkAsync(batch, ct);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
            await _repo.UpsertBulkAsync(batch, ct);
    }
}
