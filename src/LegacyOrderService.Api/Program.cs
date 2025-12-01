using LegacyOrderService.Api.Console;
using LegacyOrderService.Api.Workers;
using LegacyOrderService.Application.Commands.CreateOrder;
using LegacyOrderService.Application.Interfaces;
using LegacyOrderService.Application.Services;
using LegacyOrderService.Infrastructure.Caching;
using LegacyOrderService.Infrastructure.Persistence.Sqlite;
using LegacyOrderService.Infrastructure.Resilience;
using System.Text.Json.Serialization;
using SQLitePCL;

Batteries.Init();
var builder = WebApplication.CreateBuilder(args);

// Controllers / JSON options
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Application & domain services
builder.Services.AddSingleton<IOrderRepository, SqliteOrderRepository>();
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddSingleton<OrderProcessingChannel>();

// Command handlers
builder.Services.AddTransient<CreateOrderHandler>();

// Infrastructure (HTTP + Polly)
builder.Services.AddHttpClient("default")
    .AddPolicyHandler(ResiliencePolicies.GetResiliencePolicy());

// Caching
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

// Hosted services: console UI + background worker
builder.Services.AddHostedService<ConsoleInteractionService>(); // interactive console (like original)
builder.Services.AddHostedService<OrderWorker>();               // background consumer from channel

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Minimal API: create order
app.MapPost("/orders", async (CreateOrderCommand cmd, CreateOrderHandler handler, CancellationToken ct) =>
{
    var created = await handler.Handle(cmd, ct);
    return Results.Created($"/orders/{created.Id}", created);
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

await app.RunAsync();
