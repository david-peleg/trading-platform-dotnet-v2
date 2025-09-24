// src/TradingPlatform.Infrastructure/Common/Db.cs
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TradingPlatform.Infrastructure.Common;

/// <summary>Small helper to run stored procedures with Dapper.</summary>
public sealed class Db : IDb
{
    private readonly string _cs;

    public Db(IConfiguration configuration)
    {
        _cs = configuration.GetConnectionString("TradingPlatformNet8")
              ?? throw new InvalidOperationException("Missing connection string 'TradingPlatformNet8'.");
    }

    public async Task<int> ExecuteAsync(string spName, object? parameters, CancellationToken ct)
    {
        await using var conn = new SqlConnection(_cs);
        return await conn.ExecuteAsync(new CommandDefinition(spName, parameters,
            commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string spName, object? parameters, CancellationToken ct)
    {
        await using var conn = new SqlConnection(_cs);
        return await conn.QueryAsync<T>(new CommandDefinition(spName, parameters,
            commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    /// <summary>Create raw SqlConnection (e.g., for TVPs).</summary>
    public SqlConnection CreateConnection() => new SqlConnection(_cs);
}
