namespace TradingPlatform.Domain.Features;

/// <summary>
/// Represents a single computed feature value for a symbol on a specific trading date.
/// </summary>
public sealed record Feature(
    string Symbol,
    DateOnly Dt,
    string FeatureName,
    double Value,
    short? Window);
