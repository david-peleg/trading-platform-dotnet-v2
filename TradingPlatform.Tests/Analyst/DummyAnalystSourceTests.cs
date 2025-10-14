using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingPlatform.Infrastructure.Analyst;
using Xunit;

namespace TradingPlatform.Tests.Analyst;

/// <summary>Checks basic behavior and hash stability of DummyAnalystSource.</summary>
public sealed class DummyAnalystSourceTests
{
    [Fact]
    public async Task GetAsync_Returns_Items_In_Range()
    {
        var src = new DummyAnalystSource();
        var now = DateTime.UtcNow;
        var from = now.AddDays(-2);
        var list = await src.GetAsync("AAPL", from, now, CancellationToken.None);

        Assert.NotEmpty(list);
        Assert.All(list, x => Assert.InRange(x.PublishedAt, from, now));
    }

    [Fact]
    public async Task GetAsync_Same_Input_Produces_Same_Hashes()
    {
        var src = new DummyAnalystSource();
        var now = new DateTime(2025, 01, 10, 12, 0, 0, DateTimeKind.Utc);
        var from = now.AddDays(-2);

        var a = await src.GetAsync("MSFT", from, now, CancellationToken.None);
        var b = await src.GetAsync("MSFT", from, now, CancellationToken.None);

        Assert.Equal(a.Count, b.Count);
        Assert.True(a.Zip(b).All(p => p.First.ReportHash.SequenceEqual(p.Second.ReportHash)));
    }
}
