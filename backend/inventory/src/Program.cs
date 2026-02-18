using Microsoft.EntityFrameworkCore;
using Serilog;
using Azure.Storage.Blobs;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Clear default configuration providers to ensure only environment variables are used
builder.Configuration.Sources.Clear();
builder.Configuration.AddEnvironmentVariables();

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
var sqlConnectionString = builder.Configuration["SQL__CONNECTION_STRING"];
if (string.IsNullOrEmpty(sqlConnectionString))
{
    throw new InvalidOperationException("SQL__CONNECTION_STRING environment variable is not set.");
}
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

// Configure RabbitMQ
var rabbitMqHost = builder.Configuration["RABBITMQ__HOST"] ?? "rabbitmq";
var rabbitMqUser = builder.Configuration["RABBITMQ__USER"];
var rabbitMqPassword = builder.Configuration["RABBITMQ__PASSWORD"];

if (string.IsNullOrEmpty(rabbitMqUser) || string.IsNullOrEmpty(rabbitMqPassword))
{
    throw new InvalidOperationException("RABBITMQ__USER or RABBITMQ__PASSWORD environment variables are not set.");
}

builder.Services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory()
{
    HostName = rabbitMqHost,
    UserName = rabbitMqUser,
    Password = rabbitMqPassword,
    DispatchConsumersAsync = true
});

// Configure Azure Blob Storage Client
var azureStorageConnectionString = builder.Configuration["AZURE_STORAGE_CONNECTION_STRING"];
if (string.IsNullOrEmpty(azureStorageConnectionString))
{
    throw new InvalidOperationException("AZURE_STORAGE_CONNECTION_STRING environment variable is not set.");
}
builder.Services.AddSingleton(x => new BlobServiceClient(azureStorageConnectionString));

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(sqlConnectionString, name: "SQL-DB-Check", tags: new[] { "ready" })
    .AddRabbitMQ(
        sp => sp.GetRequiredService<IConnectionFactory>(),
        name: "RabbitMQ-Check", tags: new[] { "ready" })
    .AddAzureBlobStorage(azureStorageConnectionString, name: "Azure-Blob-Storage-Check", tags: new[] { "ready" });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map health checks
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("ready"),
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = (_) => false // Liveness check only checks if the app is running
});

app.MapGet("/", () => "Hello from Inventory!");

app.Run();
