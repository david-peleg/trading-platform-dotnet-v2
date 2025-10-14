// src/Application/Analyst/AnalystIngestionUseCase.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingPlatform.Domain.Analyst;
using TradingPlatform.Infrastructure.Analyst;
using TradingPlatform.Infrastructure.Symbols;

namespace TradingPlatform.Application.Analyst;

/// <summary>Runs one ingestion pass over active symbols from ISymbolRegistry and persists reports via repository (SP-only).</summary>
public sealed class AnalystIngestionUseCase
{
    private readonly ISymbolRegistry _symbols;
    private readonly IAnalystSource _source;
    private readonly IAnalystReportsRepository _repo;

    public AnalystIngestionUseCase(ISymbolRegistry symbols, IAnalystSource source, IAnalystReportsRepository repo)
        => (_symbols, _source, _repo) = (symbols, source, repo);

    /// <summary>Ingests analyst reports for all active tickers in range [fromUtc, toUtc] and upserts in batches.</summary>
    public async Task RunOnceAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        const int takeAll = 100_000;
        var all = await _symbols.GetAllAsync(exchange: null, take: takeAll, ct);

        // FIX: Symbol property name -> SymbolCode
        var activeSymbols = all.Where(s => s.IsActive)
                               .Select(s => s.SymbolCode)
                               .ToArray();

        var buffer = new List<AnalystReport>(capacity: 256);
        foreach (var symbol in activeSymbols)
        {
            ct.ThrowIfCancellationRequested();

            var reports = await _source.GetAsync(symbol, fromUtc, toUtc, ct);
            if (reports.Count > 0)
            {
                buffer.AddRange(reports);
                if (buffer.Count >= 200)
                {
                    await _repo.UpsertBulkAsync(buffer, ct);
                    buffer.Clear();
                }
            }
        }

        if (buffer.Count > 0)
            await _repo.UpsertBulkAsync(buffer, ct);
    }
}
