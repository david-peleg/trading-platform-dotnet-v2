namespace TradingPlatform.Domain.Prices;

/// <summary>Daily OHLCV row.</summary>
public sealed record PriceDaily(
    string Symbol,
    DateOnly Dt,
    decimal? Open,
    decimal? High,
    decimal? Low,
    decimal? Close,
    long? Volume,
    string? Source);
