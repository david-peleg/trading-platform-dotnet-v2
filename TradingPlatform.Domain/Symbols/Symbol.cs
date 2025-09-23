// src/TradingPlatform.Domain/Symbols/Symbol.cs
namespace TradingPlatform.Domain.Symbols;

/// <summary>Reference symbol row.</summary>
public sealed record Symbol(
    string SymbolCode,
    string Name,
    string Exchange,
    string Country,
    string Sector,
    bool IsActive);
