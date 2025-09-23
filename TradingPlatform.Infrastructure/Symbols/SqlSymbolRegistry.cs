// src/TradingPlatform.Infrastructure/Symbols/SqlSymbolRegistry.cs
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TradingPlatform.Domain.Symbols;
using TradingPlatform.Infrastructure.Common;
using Dapper;

namespace TradingPlatform.Infrastructure.Symbols;

/// <summary>SQL implementation (SP-only) of ISymbolRegistry.</summary>
public sealed class SqlSymbolRegistry : ISymbolRegistry
{
    private readonly Db _db;
    private readonly ILogger<SqlSymbolRegistry> _logger;

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
    }

    public async Task<IReadOnlyList<Symbol>> GetAllAsync(string? exchange, int take, CancellationToken ct)
    {
        var rows = await _db.QueryAsync<SymbolRow>("dbo.Symbols_GetAll", new { Exchange = exchange, Take = take }, ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<Symbol>> SearchAsync(string query, int take, CancellationToken ct)
    {
        var rows = await _db.QueryAsync<SymbolRow>("dbo.Symbols_Search", new { Query = query, Take = take }, ct);
        return rows.Select(Map).ToList();
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
