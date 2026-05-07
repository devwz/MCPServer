using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Text.Json;

internal static class SqlQueryExecutor
{
    public static string ExecuteSelectRawJson(string connectionString, string query)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand(query, connection);
        using var reader = command.ExecuteReader();

        var results = new List<Dictionary<string, object>>();
        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }
            results.Add(row);
        }

        return JsonSerializer.Serialize(results);
    }

    public static async Task<object> ExecuteSelectAsync(string connectionString, string query)
    {
        string upper = query.ToUpperInvariant();
        if (!upper.Contains("OFFSET") && !upper.Contains("FETCH") && !upper.Contains("TOP"))
        {
            query = query.Trim().TrimEnd(';') + " OFFSET 0 ROWS FETCH NEXT 200 ROWS ONLY;";
        }

        Stopwatch sw = Stopwatch.StartNew();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var cmd = new SqlCommand(query, connection) { CommandTimeout = 30 };
        await using var reader = await cmd.ExecuteReaderAsync();

        var columns = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList();
        var rows = new List<Dictionary<string, object?>>();

        while (await reader.ReadAsync())
        {
            Dictionary<string, object?> row = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            rows.Add(row);
        }

        sw.Stop();
        return new
        {
            Columns = columns,
            Rows = rows,
            RowCount = rows.Count,
            ExecutionTimeMs = sw.Elapsed.TotalMilliseconds
        };
    }

    public static async Task<object> ExecuteNonQueryAsync(string connectionString, string query)
    {
        Stopwatch sw = Stopwatch.StartNew();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var sqlTransaction = (SqlTransaction)await connection.BeginTransactionAsync();
        try
        {
            await using var cmd = new SqlCommand(query, connection, sqlTransaction) { CommandTimeout = 30 };
            int affectedRows = await cmd.ExecuteNonQueryAsync();
            await sqlTransaction.CommitAsync();
            sw.Stop();
            return new
            {
                AffectedRows = affectedRows,
                ExecutionTimeMs = sw.Elapsed.TotalMilliseconds
            };
        }
        catch
        {
            await sqlTransaction.RollbackAsync();
            sw.Stop();
            throw;
        }
    }
}