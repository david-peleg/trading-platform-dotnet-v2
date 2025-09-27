using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TradingPlatform.Domain.Ingestion;
using TradingPlatform.Infrastructure.News;
using Xunit;

public sealed class RssParsingTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string _rss;
        public StubHandler(string rss) => _rss = rss;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(_rss) });
    }

    [Fact]
    public async Task Parse_Rss_To_RawNews()
    {
        var rss = """
        <rss version="2.0"><channel>
          <title>DemoFeed</title>
          <item>
            <title>Apple (AAPL) announces event</title>
            <link>https://news/site/aapl</link>
            <pubDate>Mon, 01 Jan 2024 10:00:00 GMT</pubDate>
          </item>
        </channel></rss>
        """;

        var http = new HttpClient(new StubHandler(rss));
        var f = new TestHttpClientFactory(http);
        var opts = Options.Create(new IngestionOptions { Feeds = new[] { "https://stub/rss" } });
        var src = new RssNewsSource(f, opts, NullLogger<RssNewsSource>.Instance);

        var from = new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2024, 01, 02, 0, 0, 0, DateTimeKind.Utc);

        var list = new List<TradingPlatform.Domain.News.RawNews>();
        await foreach (var n in src.ReadAsync(from, to, CancellationToken.None))
            list.Add(n);

        Assert.Single(list);
        Assert.Equal("DemoFeed", list[0].Source);
        Assert.Equal("https://news/site/aapl", list[0].Url);
        Assert.NotNull(list[0].BodyHash);
        Assert.True(list[0].BodyHash.Length == 32);
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;
        public TestHttpClientFactory(HttpClient c) => _client = c;
        public HttpClient CreateClient(string name) => _client;
    }
}
