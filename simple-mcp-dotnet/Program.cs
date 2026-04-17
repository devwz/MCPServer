using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;

var server = McpServer.Create(new StdioServerTransport("simple-mcp-dotnet"), new McpServerOptions
{
    ServerInfo = new Implementation
    {
        Name = "simple-mcp-dotnet",
        Version = "1.0.0"
    },
    ToolCollection = new McpServerPrimitiveCollection<McpServerTool>()
    {
        McpServerTool.Create(
            () => "Hello, World",
            new McpServerToolCreateOptions
            {
                Name = "hello_world",
                Description = "Returns Hello, World"
            }
        )
    }
});

await server.RunAsync();