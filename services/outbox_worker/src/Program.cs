using OutboxWorker.Infrastructure;
using OutboxWorker.Services;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.EntityFrameworkCore;

namespace OutboxWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/outbox-worker-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting Outbox Worker host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    // Configure OpenTelemetry
                    services.AddOpenTelemetry()
                        .WithTracing(builder => builder
                            .AddSource("OutboxWorker")
                            .SetResourceBuilder(
                                ResourceBuilder.CreateDefault()
                                    .AddService(serviceName: "OutboxWorker", serviceVersion: "1.0.0"))
                            .AddAspNetCoreInstrumentation()
                            .AddEntityFrameworkCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddConsoleExporter());

                    // Configure DbContext
                    var sqlConnectionString = configuration["SQL_CONNECTION_STRING"];
                    if (string.IsNullOrEmpty(sqlConnectionString))
                    {
                        Log.Fatal("SQL_CONNECTION_STRING environment variable is not set.");
                        throw new InvalidOperationException("SQL_CONNECTION_STRING environment variable is not set.");
                    }
                    services.AddDbContext<OutboxDbContext>(options =>
                        options.UseSqlServer(sqlConnectionString));

                    // Configure RabbitMQ Publisher
                    var rabbitMqConnectionString = configuration["RABBITMQ_CONNECTION_STRING"];
                    if (string.IsNullOrEmpty(rabbitMqConnectionString))
                    {
                        Log.Fatal("RABBITMQ_CONNECTION_STRING environment variable is not set.");
                        throw new InvalidOperationException("RABBITMQ_CONNECTION_STRING environment variable is not set.");
                    }
                    services.AddSingleton(new RabbitMqPublisher(rabbitMqConnectionString));

                    // Register OutboxProcessor as a hosted service
                    services.AddHostedService<OutboxProcessor>();
                });
    }
}
