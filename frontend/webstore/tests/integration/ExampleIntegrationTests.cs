using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using Webstore; // Assuming your Webstore project namespace

namespace Webstore.IntegrationTests
{
    public class ExampleIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public ExampleIntegrationTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/"); // Adjust to a valid endpoint in your Webstore app

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            response.Content.Headers.ContentType.ToString().Should().Contain("text/html"); // Adjust content type as needed
        }
    }
}
