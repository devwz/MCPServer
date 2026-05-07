using ModelContextProtocol.Server;
using System.Text.Json;

internal static class SqlToolRegistry
{
    public static McpServerPrimitiveCollection<McpServerTool> Create(string connectionString)
    {
        return new McpServerPrimitiveCollection<McpServerTool>()
        {
            McpServerTool.Create(
                (string query) => SqlQueryExecutor.ExecuteSelectRawJson(connectionString, query),
                new McpServerToolCreateOptions
                {
                    Name = "execute_select_query",
                    Description = "Execute a SELECT query and return the results as a JSON string"
                }
            ),
            McpServerTool.Create(
                async () => await SchemaReader.GetSchemaJsonAsync(connectionString),
                new McpServerToolCreateOptions
                {
                    Name = "get_schema",
                    Description = """
                        Get the full database schema as a JSON string: all tables in the 'dbo' schema, with their columns (name, data type, nullable, primary key flag) and foreign keys (referenced table and column).
                        Call this tool first whenever you want to understand the database structure before building SQL queries.
                        """
                }
            ),
            McpServerTool.Create(
                async (string query) =>
                {
                    SqlQueryValidator.Validate(query, "SELECT");
                    var result = await SqlQueryExecutor.ExecuteSelectAsync(connectionString, query);
                    return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                },
                new McpServerToolCreateOptions
                {
                    Name = "sql_select",
                    Description = """
                        Execute a SQL SELECT query and return the results as a JSON string.
                        Only SELECT statements are allowed.
                        Use the 'get_schema' tool to understand the database structure and write correct queries.
                        A default pagination of 200 rows is applied if the query doesn't specify one.
                        """
                }
            ),
            McpServerTool.Create(
                async (string query) =>
                {
                    SqlQueryValidator.Validate(query, "INSERT");
                    var result = await SqlQueryExecutor.ExecuteNonQueryAsync(connectionString, query);
                    return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                },
                new McpServerToolCreateOptions
                {
                    Name = "sql_insert",
                    Description = """
                        Execute a SQL INSERT query and return the number of affected rows as a JSON string.
                        Only INSERT statements are allowed.
                        Use the 'get_schema' tool to understand the database structure and write correct queries.
                        """
                }
            ),
            McpServerTool.Create(
                async (string query) =>
                {
                    SqlQueryValidator.Validate(query, "UPDATE");
                    var result = await SqlQueryExecutor.ExecuteNonQueryAsync(connectionString, query);
                    return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                },
                new McpServerToolCreateOptions
                {
                    Name = "sql_update",
                    Description = """
                        Execute a SQL UPDATE query and return the number of affected rows as a JSON string.
                        Only UPDATE statements are allowed.
                        Use the 'get_schema' tool to understand the database structure and write correct queries.
                        """
                }
            ),
            McpServerTool.Create(
                async (string query) =>
                {
                    SqlQueryValidator.Validate(query, "DELETE");
                    var result = await SqlQueryExecutor.ExecuteNonQueryAsync(connectionString, query);
                    return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                },
                new McpServerToolCreateOptions
                {
                    Name = "sql_delete",
                    Description = """
                        Execute a SQL DELETE query and return the number of affected rows as a JSON string.
                        Only DELETE statements are allowed.
                        Use the 'get_schema' tool to understand the database structure and write correct queries.
                        """
                }
            )
        };
    }
}