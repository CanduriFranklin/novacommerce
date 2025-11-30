using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using PactNet;
using PactNet.Infrastructure.Outputters;
using Xunit;
using Xunit.Abstractions;

namespace NovaCommerce.Inventory.ContractTests
{
    public class InventoryApiProviderTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IWebHost _webHost;
        private readonly string _pactFilePath;

        public InventoryApiProviderTests(ITestOutputHelper output)
        {
            _output = output;
            _webHost = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:9000")
                .Build();
            _webHost.Start();

            _pactFilePath = Path.Combine("..", "..", "..", "..", "sales", "tests", "pacts", "SalesService-InventoryService.json");
        }

        [Fact]
        public void EnsureInventoryApiHonoursPactWithSalesService()
        {
            // Arrange
            var config = new PactVerifierConfig
            {
                Outputters = new IOutputter[] { new XUnitOutputter(_output) },
                Verbose = true
            };

            // Act & Assert
            new PactVerifier(config)
                .ServiceProvider("InventoryService", new Uri("http://localhost:9000"))
                .WithFileSource(new FileInfo(_pactFilePath))
                .Verify();
        }

        public void Dispose()
        {
            _webHost.Dispose();
        }
    }
}
