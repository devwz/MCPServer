var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<HelloWorldTool>()
    .WithTools<RandomNumberTool>()
    .WithTools<CheckEvenOrOddTool>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapMcp();

app.Run();