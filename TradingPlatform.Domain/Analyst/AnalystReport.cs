namespace TradingPlatform.Domain.Analyst;

/// <summary>Analyst/research report normalized row for persistence and queries.</summary>
public sealed record AnalystReport(
    string Symbol,
    string Firm,
    string? Title,
    string? Action,
    string? RatingFrom,
    string? RatingTo,
    decimal? TargetPrice,
    string? Currency,
    string Url,
    string Source,
    System.DateTime PublishedAt,
    byte[] ReportHash,
    string? Lang
);
