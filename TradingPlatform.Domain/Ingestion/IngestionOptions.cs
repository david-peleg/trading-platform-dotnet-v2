namespace TradingPlatform.Domain.Ingestion;

/// <summary>
/// Options for ingestion sources (e.g., RSS feed URLs).
/// </summary>
public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";
    public string[] Feeds { get; init; } = Array.Empty<string>();
}
