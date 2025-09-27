using TradingPlatform.Infrastructure.News;
using Xunit;

public sealed class HashTests
{
    [Fact]
    public void Same_Input_Same_Hash()
    {
        var dt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var h1 = NewsHash.Compute("H", "U", "S", dt);
        var h2 = NewsHash.Compute("H", "U", "S", dt);
        Assert.Equal(h1, h2);
    }

    [Fact]
    public void Change_One_Field_Changes_Hash()
    {
        var dt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var h1 = NewsHash.Compute("H", "U", "S", dt);
        var h2 = NewsHash.Compute("H2", "U", "S", dt);
        Assert.NotEqual(h1, h2);
    }
}
