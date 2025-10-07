// src/TradingPlatform.Api/Program.cs
using System;
using Carter;

// Prices (P2)
using TradingPlatform.Application.Prices;
using TradingPlatform.Infrastructure.Prices;

// News (P3)
using TradingPlatform.Application.News;
using TradingPlatform.Domain.Ingestion;
using TradingPlatform.Infrastructure.News;

// Symbols (P1)
using TradingPlatform.Infrastructure.Symbols;

// Common DB
using TradingPlatform.Infrastructure.Common;

// Filings (P4)
using TradingPlatform.Application.Ingestion;
using TradingPlatform.Domain.Filings;
using TradingPlatform.Infrastructure.Filings;

var builder = WebApplication.CreateBuilder(args);

// ========== Services (DI) ==========
builder.Services.AddCarter();

// DB helper
builder.Services.AddSingleton<IDb, Db>();
builder.Services.AddSingleton(sp => (Db)sp.GetRequiredService<IDb>());

// ---------- Symbols (Prompt 1) ----------
builder.Services.AddSingleton<ISymbolRegistry, SqlSymbolRegistry>();

// ---------- Prices (Prompt 2) ----------
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

// ---------- News (Prompt 3) ----------
builder.Services.Configure<IngestionOptions>(builder.Configuration.GetSection(IngestionOptions.SectionName));
builder.Services.AddHttpClient(nameof(RssNewsSource)).SetHandlerLifetime(TimeSpan.FromMinutes(5));
builder.Services.AddSingleton<INewsRepository>(sp =>
    new SqlNewsRepository(builder.Configuration.GetConnectionString("TradingPlatformNet8")!));
builder.Services.AddSingleton<INewsSource, RssNewsSource>();
builder.Services.AddSingleton<NewsIngestionUseCase>();
builder.Services.AddSingleton<NewsBackfillJob>();
if (builder.Configuration.GetValue("News:EnableDailyJob", false))
    builder.Services.AddHostedService<NewsDailyJob>();

// ---------- Filings (Prompt 4) ----------
builder.Services.AddSingleton<IFilingsRepository>(sp =>
    new SqlFilingsRepository(builder.Configuration.GetConnectionString("TradingPlatformNet8")!));
builder.Services.AddSingleton<IFilingsSource, DummyFilingsSource>();
builder.Services.AddScoped<FilingsBackfillJob>();
builder.Services.AddScoped<FilingsDailyJob>();
if (builder.Configuration.GetValue("Filings:EnableDailyJob", true))
    builder.Services.AddHostedService<FilingsDailyWorker>();

// ========== App ==========
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapCarter();

app.Run();
