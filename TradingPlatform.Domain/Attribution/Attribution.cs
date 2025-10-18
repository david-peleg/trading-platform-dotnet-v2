namespace TradingPlatform.Domain.Attribution;

/// <summary>Attribution link from an event to a symbol with direction/confidence/horizon.</summary>
public sealed record Attribution(
    ItemType ItemType,
    long ItemId,
    string Symbol,
    short Direction,   // -1/0/+1
    double Confidence, // 0..1
    short HorizonD     // e.g., 1/7/30/60
);
