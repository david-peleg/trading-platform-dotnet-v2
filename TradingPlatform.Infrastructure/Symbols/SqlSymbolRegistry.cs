// src/TradingPlatform.Infrastructure/Symbols/SqlSymbolRegistry.cs
using System.Collections.Immutable;
using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TradingPlatform.Domain.Symbols;
using TradingPlatform.Infrastructure.Common;

namespace TradingPlatform.Infrastructure.Symbols;

/// <summary>SQL implementation (SP-only) of ISymbolRegistry with in-memory cache for fast matching.</summary>
public sealed class SqlSymbolRegistry : ISymbolRegistry
{
    private readonly Db _db;
    private readonly ILogger<SqlSymbolRegistry> _logger;

    // In-memory cache of active symbols for matching.
    private ImmutableArray<Symbol> _cache = ImmutableArray<Symbol>.Empty;

    public SqlSymbolRegistry(Db db, ILogger<SqlSymbolRegistry> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task UpsertAsync(IEnumerable<Symbol> items, CancellationToken ct)
    {
        var tvp = BuildSymbolsTvp(items);

        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand("dbo.Symbols_UpsertBulk", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        var p = new SqlParameter("@Rows", SqlDbType.Structured)
        {
            TypeName = "dbo.SymbolType",
            Value = tvp
        };
        cmd.Parameters.Add(p);

        _ = await cmd.ExecuteNonQueryAsync(ct);

        // Refresh cache with the latest active set we just upserted.
        // Keep only active ones to reduce noise in matching.
        _cache = items.Where(s => s.IsActive).ToImmutableArray();
    }

    public async Task<IReadOnlyList<Symbol>> GetAllAsync(string? exchange, int take, CancellationToken ct)
    {
        var rows = await _db.QueryAsync<SymbolRow>("dbo.Symbols_GetAll", new { Exchange = exchange, Take = take }, ct);
        var list = rows.Select(Map).ToList();

        // If cache is empty, initialize it lazily from DB call.
        if (_cache.IsDefaultOrEmpty)
            _cache = list.Where(s => s.IsActive).ToImmutableArray();

        return list;
    }

    public async Task<IReadOnlyList<Symbol>> SearchAsync(string query, int take, CancellationToken ct)
    {
        var rows = await _db.QueryAsync<SymbolRow>("dbo.Symbols_Search", new { Query = query, Take = take }, ct);
        return rows.Select(Map).ToList();
    }

    /// <summary>
    /// Heuristics:
    /// 1) Ticker in parentheses, e.g. "(AAPL)".
    /// 2) Whole-word ticker match against active symbols.
    /// 3) Fallback: company-name substring match.
    /// </summary>
    public async Task<string?> TryMatchAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Ensure cache is warm (lazy-load a reasonable batch if empty).
        if (_cache.IsDefaultOrEmpty)
        {
            try
            {
                _ = await GetAllAsync(exchange: null, take: 5000, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed warming symbol cache; continuing with empty cache.");
            }
        }

        var snapshot = _cache;
        if (snapshot.IsDefaultOrEmpty) return null;

        // 1) (TICKER)
        var m = Regex.Match(text, @"\(([A-Z]{1,10})\)");
        if (m.Success)
        {
            var t = m.Groups[1].Value;
            if (snapshot.Any(s => s.SymbolCode.Equals(t, StringComparison.OrdinalIgnoreCase)))
                return t;
        }

        // 2) whole-word ticker
        foreach (var s in snapshot)
        {
            var pattern = $@"\b{Regex.Escape(s.SymbolCode)}\b";
            if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                return s.SymbolCode;
        }

        // 3) company name fragment (coarse)
        foreach (var s in snapshot)
        {
            if (!string.IsNullOrWhiteSpace(s.Name) &&
                text.Contains(s.Name, StringComparison.OrdinalIgnoreCase))
                return s.SymbolCode;
        }

        return null;
    }

    private static Symbol Map(SymbolRow r) =>
        new(r.Symbol, r.Name ?? string.Empty, r.Exchange ?? string.Empty, r.Country ?? string.Empty, r.Sector ?? string.Empty, r.IsActive);

    private sealed class SymbolRow
    {
        public string Symbol { get; init; } = string.Empty;
        public string? Name { get; init; }
        public string? Exchange { get; init; }
        public string? Country { get; init; }
        public string? Sector { get; init; }
        public bool IsActive { get; init; }
    }

    /// <summary>Builds the TVP DataTable for dbo.SymbolType. Public for unit-tests.</summary>
    public static DataTable BuildSymbolsTvp(IEnumerable<Symbol> items)
    {
        var table = new DataTable();
        table.Columns.Add("Symbol", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Exchange", typeof(string));
        table.Columns.Add("Country", typeof(string));
        table.Columns.Add("Sector", typeof(string));
        table.Columns.Add("IsActive", typeof(bool));

        foreach (var s in items)
            table.Rows.Add(s.SymbolCode, s.Name, s.Exchange, s.Country, s.Sector, s.IsActive);

        return table;
    }
}
