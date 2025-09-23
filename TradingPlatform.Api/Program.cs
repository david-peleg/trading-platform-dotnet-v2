var builder = WebApplication.CreateBuilder(args);
// Program.cs – DI
builder.Services.AddSingleton<TradingPlatform.Infrastructure.Common.Db>();
builder.Services.AddSingleton<TradingPlatform.Infrastructure.Symbols.ISymbolRegistry,
                              TradingPlatform.Infrastructure.Symbols.SqlSymbolRegistry>();

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();
