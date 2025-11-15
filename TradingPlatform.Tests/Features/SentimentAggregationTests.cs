using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace TradingPlatform.Tests.Features;

/// <summary>
/// Tests daily sentiment aggregation (Direction * Confidence) logic.
/// </summary>
public sealed class SentimentAggregationTests
{
    private sealed record AttributionSample(DateOnly Dt, double Direction, double Confidence);

    [Fact]
    public void Sentiment_AggregatesByDateUsingAverageProduct()
    {
        var dt = new DateOnly(2024, 1, 1);

        var items = new List<AttributionSample>
        {
            new(dt, 1, 0.8),
            new(dt, 1, 0.6),
            new(dt, -1, 0.2)
        };

        var grouped = items
            .GroupBy(a => a.Dt)
            .Select(g => g.Average(x => x.Direction * x.Confidence))
            .Single();

        grouped.Should().BeApproximately((0.8 + 0.6 - 0.2) / 3d, 1e-9);
    }
}
