using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;

const string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=inventory-mkt;Integrated Security=SSPI";

McpServer server = McpServer.Create(
    new StdioServerTransport("dotnet-sqlclient-mcp"),
    new McpServerOptions
    {
        ServerInfo = new Implementation
        {
            Name = "dotnet-sqlclient-mcp",
            Description = "A simple MCP server that exposes SQL query execution and schema reading tools for a Microsoft SQL Server database, using the Microsoft.Data.SqlClient library.",
            Version = "1.0.0"
        },
        ToolCollection = SqlToolRegistry.Create(connectionString)
    });

await server.RunAsync();