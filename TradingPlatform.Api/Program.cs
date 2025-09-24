// src/TradingPlatform.Api/Program.cs
using Carter;
using TradingPlatform.Application.Prices;
using TradingPlatform.Infrastructure.Common;
using TradingPlatform.Infrastructure.Prices;
using TradingPlatform.Infrastructure.Symbols;

var builder = WebApplication.CreateBuilder(args);

// === Services (DI) ===
builder.Services.AddCarter();

// DB helper: נרשום גם כ-IDb וגם כ-Db (יש טיפוסים שדורשים את הקונקרטי)
builder.Services.AddSingleton<IDb, Db>();
builder.Services.AddSingleton<Db>(sp => (Db)sp.GetRequiredService<IDb>());

// Symbols registry (Prompt 1)
builder.Services.AddSingleton<ISymbolRegistry, SqlSymbolRegistry>();

// Prices (Prompt 2): בחירה לפי קונפיג
var dbMode = builder.Configuration["Db:Mode"] ?? "Real";
if (string.Equals(dbMode, "Noop", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IPriceRepository, NoopPriceRepository>();
}
else
{
    builder.Services.AddSingleton<IPriceRepository, SqlPriceRepository>();
}
builder.Services.AddSingleton<IPriceDataSource, DummyPriceDataSource>();

// Jobs
builder.Services.AddSingleton<PricesBackfillJob>();
builder.Services.AddSingleton<PricesDailyJob>();
var enableDaily = builder.Configuration.GetValue("Prices:EnableDailyJob", true);
if (enableDaily)
{
    // hosted service מאותו Singleton
    builder.Services.AddHostedService(sp => sp.GetRequiredService<PricesDailyJob>());
}

// === App ===
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapCarter();

app.Run();
