using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.EntityFrameworkCore;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.Sources.Clear();
        config.AddEnvironmentVariables();
    })
    .UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext())
    .ConfigureServices((hostContext, services) =>
    {
        var sqlConnectionString = hostContext.Configuration["SQL__CONNECTION_STRING"];
        if (string.IsNullOrEmpty(sqlConnectionString))
        {
            throw new InvalidOperationException("SQL__CONNECTION_STRING environment variable is not set.");
        }
        services.AddDbContext<DbAgentDbContext>(options =>
            options.UseSqlServer(sqlConnectionString));
        // Add other services for the DB agent here
    })
    .Build();

Log.Information("DbAgent started.");

await host.RunAsync();
