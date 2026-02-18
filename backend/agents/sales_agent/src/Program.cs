using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using RabbitMQ.Client;

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
        // Configure RabbitMQ
        var rabbitMqHost = hostContext.Configuration["RABBITMQ__HOST"] ?? "rabbitmq";
        var rabbitMqUser = hostContext.Configuration["RABBITMQ__USER"];
        var rabbitMqPassword = hostContext.Configuration["RABBITMQ__PASSWORD"];

        if (string.IsNullOrEmpty(rabbitMqUser) || string.IsNullOrEmpty(rabbitMqPassword))
        {
            throw new InvalidOperationException("RABBITMQ__USER or RABBITMQ__PASSWORD environment variables are not set.");
        }

        services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory()
        {
            HostName = rabbitMqHost,
            UserName = rabbitMqUser,
            Password = rabbitMqPassword,
            DispatchConsumersAsync = true
        });

        // Add other services for the Sales agent here
    })
    .Build();

Log.Information("SalesAgent started.");

await host.RunAsync();
