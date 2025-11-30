using Serilog;
using Serilog.Formatting.Json;
using NovaCommerce.Services.Gateway; // Startup namespace (PascalCase)

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting NovaCommerce.Gateway");

    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            // Ocelot routing config
            config.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
        })
        .UseSerilog((context, services, logger) => logger
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(new JsonFormatter()))
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .Build()
        .Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "NovaCommerce.Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
