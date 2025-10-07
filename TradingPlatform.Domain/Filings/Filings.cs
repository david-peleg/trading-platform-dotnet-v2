namespace TradingPlatform.Domain.Filings;

/// <summary>Represents a single corporate filing/financial report entry.</summary>
public sealed record Filing(
    string Symbol,
    string FilingType,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd,
    string Url,
    string Source,
    DateTime PublishedAtUtc,
    byte[] DocHash,
    string? Lang);
