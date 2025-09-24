// src/TradingPlatform.Api/Program.cs
using Carter;
using TradingPlatform.Infrastructure.Common;
using TradingPlatform.Infrastructure.Symbols;

var builder = WebApplication.CreateBuilder(args);

// DI
builder.Services.AddCarter();
builder.Services.AddSingleton<Db>();
builder.Services.AddSingleton<ISymbolRegistry, SqlSymbolRegistry>();

var app = builder.Build();

// Health + Carter endpoints (SymbolsEndpoint)
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapCarter();

app.Run();
