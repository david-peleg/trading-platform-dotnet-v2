using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace TradingPlatform.Tests.Features;

/// <summary>
/// Tests basic rolling-window calculations (MA/RSI/Volatility) on synthetic series.
/// </summary>
public sealed class RollingCalculationsTests
{
    [Fact]
    public void SimpleMovingAverage_ComputesExpectedValue()
    {
        var values = Enumerable.Range(1, 10).Select(x => (double)x).ToArray();
        var window = 5;

        var ma = ComputeSimpleMovingAverage(values, window).ToArray();

        ma.Length.Should().Be(6);
        ma[0].Should().Be((1 + 2 + 3 + 4 + 5) / 5d);
        ma[^1].Should().Be((6 + 7 + 8 + 9 + 10) / 5d);
    }

    [Fact]
    public void Rsi_ComputesInRange()
    {
        var values = new double[] { 10, 11, 12, 13, 12, 11, 12, 13, 14, 15, 14, 13, 14, 15, 16 };
        var rsi = ComputeRsi(values, 14).Single();

        rsi.Should().BeGreaterThan(0).And.BeLessThan(100);
    }

    [Fact]
    public void Volatility_ComputesPositiveStdev()
    {
        var values = Enumerable.Range(1, 40).Select(x => 100d + x).ToArray();
        var vol = ComputeVolatility(values, 20).Last();

        vol.Should().BeGreaterThan(0);
    }

    private static IEnumerable<double> ComputeSimpleMovingAverage(double[] values, int window)
    {
        double sum = 0;
        for (var i = 0; i < values.Length; i++)
        {
            sum += values[i];
            if (i >= window)
            {
                sum -= values[i - window];
            }

            if (i >= window - 1)
            {
                yield return sum / window;
            }
        }
    }

    private static IEnumerable<double> ComputeRsi(double[] values, int period)
    {
        if (values.Length <= period)
        {
            yield break;
        }

        var gains = new double[values.Length];
        var losses = new double[values.Length];

        for (var i = 1; i < values.Length; i++)
        {
            var change = values[i] - values[i - 1];
            if (change > 0)
            {
                gains[i] = change;
            }
            else
            {
                losses[i] = -change;
            }
        }

        double avgGain = 0;
        double avgLoss = 0;

        for (var i = 1; i <= period; i++)
        {
            avgGain += gains[i];
            avgLoss += losses[i];
        }

        avgGain /= period;
        avgLoss /= period;

        for (var i = period; i < values.Length; i++)
        {
            if (i > period)
            {
                avgGain = (avgGain * (period - 1) + gains[i]) / period;
                avgLoss = (avgLoss * (period - 1) + losses[i]) / period;
            }

            if (avgLoss == 0)
            {
                yield return 100.0;
            }
            else
            {
                var rs = avgGain / avgLoss;
                var rsi = 100.0 - 100.0 / (1.0 + rs);
                yield return rsi;
            }
        }
    }

    private static IEnumerable<double> ComputeVolatility(double[] values, int window)
    {
        if (values.Length <= window)
        {
            yield break;
        }

        var returns = new double[values.Length];
        for (var i = 1; i < values.Length; i++)
        {
            returns[i] = Math.Log(values[i] / values[i - 1]);
        }

        for (var i = window; i < values.Length; i++)
        {
            var slice = returns[(i - window + 1)..(i + 1)];
            var mean = slice.Average();
            var variance = slice.Select(r => (r - mean) * (r - mean)).Average();
            yield return Math.Sqrt(variance);
        }
    }
}
