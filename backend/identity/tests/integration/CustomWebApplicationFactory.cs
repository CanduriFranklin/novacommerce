using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaCommerce.Identity.Infrastructure;

namespace NovaCommerce.Identity.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                var integrationConfig = new ConfigurationBuilder()
                    .AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("JWT_SECRET", "this-is-a-test-secret-key-for-jwt"),
                        new KeyValuePair<string, string>("SQL_CONNECTION_STRING", "Server=(localdb)\\\\mssqllocaldb;Database=NovaCommerce.Identity.Tests;Trusted_Connection=True;MultipleActiveResultSets=true"),
                        new KeyValuePair<string, string>("APPLICATIONINSIGHTS_CONNECTION_STRING", "") // Disable App Insights for tests
                    })
                    .Build();
                config.AddConfiguration(integrationConfig);
            });

            builder.ConfigureServices(services =>
            {
                // Remove the app's IdentityDbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add IdentityDbContext using an in-memory database for testing.
                services.AddDbContext<IdentityDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryIdentityDb");
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database contexts
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<IdentityDbContext>();

                    // Ensure the database is created.
                    db.Database.EnsureCreated();
                }
            });
        }
    }
}
