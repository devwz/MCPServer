using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;

Random random = new ();

McpServer server = McpServer.Create(new StdioServerTransport("simple-mcp-dotnet"), new McpServerOptions
{
    ServerInfo = new Implementation
    {
        Name = "simple-mcp-dotnet",
        Version = "1.0.0"
    },
    ToolCollection = new McpServerPrimitiveCollection<McpServerTool>()
    {
        McpServerTool.Create(
            () => $"Hello, world!",
            new McpServerToolCreateOptions
            {
                Name = "hello_world",
                Description = "Return a greeting message"
            }
        ),
        McpServerTool.Create(
            (int number) => random.Next(1, number + 1),
            new McpServerToolCreateOptions
            {
                Name = "get_random_number",
                Description = "Get a random number between 1 and the given number"
            }
        ),
        McpServerTool.Create(
            (int number) => number % 2 == 0 ? "even" : "odd",
            new McpServerToolCreateOptions
            {
                Name = "check_even_or_odd",
                Description = "Check if a number is even or odd"
            }
        )
    }
});

await server.RunAsync();