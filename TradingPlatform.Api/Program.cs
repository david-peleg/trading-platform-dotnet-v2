// src/TradingPlatform.Api/Program.cs
using Carter;
using TradingPlatform.Application.Prices;
using TradingPlatform.Application.News;
using TradingPlatform.Domain.Ingestion;
using TradingPlatform.Infrastructure.Common;
using TradingPlatform.Infrastructure.Prices;
using TradingPlatform.Infrastructure.Symbols;
using TradingPlatform.Infrastructure.News;

var builder = WebApplication.CreateBuilder(args);

// === Services (DI) ===
builder.Services.AddCarter();

// DB helper
builder.Services.AddSingleton<IDb, Db>();
builder.Services.AddSingleton(sp => (Db)sp.GetRequiredService<IDb>());

// Symbols (Prompt 1)
builder.Services.AddSingleton<ISymbolRegistry, SqlSymbolRegistry>();

// Prices (Prompt 2)
var dbMode = builder.Configuration["Db:Mode"] ?? "Real";
if (string.Equals(dbMode, "Noop", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<IPriceRepository, NoopPriceRepository>();
else
    builder.Services.AddSingleton<IPriceRepository, SqlPriceRepository>();
builder.Services.AddSingleton<IPriceDataSource, DummyPriceDataSource>();
builder.Services.AddSingleton<PricesBackfillJob>();
builder.Services.AddSingleton<PricesDailyJob>();
if (builder.Configuration.GetValue("Prices:EnableDailyJob", true))
    builder.Services.AddHostedService(sp => sp.GetRequiredService<PricesDailyJob>());

// News (Prompt 3)
builder.Services.Configure<IngestionOptions>(builder.Configuration.GetSection(IngestionOptions.SectionName));
builder.Services.AddHttpClient(nameof(RssNewsSource)).SetHandlerLifetime(TimeSpan.FromMinutes(5));
builder.Services.AddSingleton<INewsRepository>(sp =>
    new SqlNewsRepository(builder.Configuration.GetConnectionString("TradingPlatformNet8")!));
builder.Services.AddSingleton<INewsSource, RssNewsSource>();
builder.Services.AddSingleton<NewsIngestionUseCase>();
builder.Services.AddSingleton<NewsBackfillJob>();

var enableNewsDaily = builder.Configuration.GetValue("News:EnableDailyJob", false); // default=false
if (enableNewsDaily)
    builder.Services.AddHostedService<NewsDailyJob>();

// === App ===
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapCarter();

app.Run();
