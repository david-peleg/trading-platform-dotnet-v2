using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradingPlatform.Domain.Analyst;

namespace TradingPlatform.Infrastructure.Analyst;

/// <summary>External analyst/research data source abstraction.</summary>
public interface IAnalystSource
{
    Task<IReadOnlyList<AnalystReport>> GetAsync(string symbol, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
}
