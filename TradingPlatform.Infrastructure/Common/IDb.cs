using Microsoft.Data.SqlClient;

namespace TradingPlatform.Infrastructure.Common;

public interface IDb
{
    Task<int> ExecuteAsync(string spName, object? parameters, CancellationToken ct);
    Task<IEnumerable<T>> QueryAsync<T>(string spName, object? parameters, CancellationToken ct);
    SqlConnection CreateConnection();
}
