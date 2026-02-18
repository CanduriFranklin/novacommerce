using Serilog;
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

// Ensure GEMINI__API_KEY is present
var geminiApiKey = builder.Configuration["GEMINI__API_KEY"];
if (string.IsNullOrEmpty(geminiApiKey))
{
    throw new InvalidOperationException("GEMINI__API_KEY environment variable is not set.");
}
// You might want to register this key for injection into other services
builder.Services.AddSingleton(geminiApiKey); // Example: Register as a singleton string

// Add Health Checks
builder.Services.AddHealthChecks(); // Basic health check

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

app.MapGet("/", () => "Hello from Gemini Agent!");

app.Run();
