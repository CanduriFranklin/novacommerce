using System;
using System.Net.Http;
using System.Threading.Tasks;
using PactNet;
using PactNet.Matchers;
using Xunit;

namespace NovaCommerce.Sales.ContractTests
{
    public class InventoryApiConsumerTests
    {
        private readonly IPactBuilderV4 _pactBuilder;

        public InventoryApiConsumerTests()
        {
            var pact = Pact.V4("SalesService", "InventoryService", new PactConfig());
            _pactBuilder = pact.UsingNativeBackend();
        }

        [Fact]
        public async Task GetProduct_WhenProductExists_ReturnsProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _pactBuilder
                .UponReceiving("A GET request to retrieve a product")
                    .Given("a product with a specific ID exists")
                    .WithRequest(HttpMethod.Get, $"/api/products/{productId}")
                .WillRespond()
                    .WithStatus(System.Net.HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-f")
                    .WithJsonBody(new
                    {
                        productId = Match.Type(productId),
                        name = Match.Type("Test Product"),
                        stock = Match.Type(10)
                    });

            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Act
                var client = new HttpClient { BaseAddress = ctx.MockServerUri };
                var response = await client.GetAsync($"/api/products/{productId}");
                response.EnsureSuccessStatusCode();

                // Assert
                Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async Task GetProduct_WhenProductDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _pactBuilder
                .UponReceiving("A GET request for a non-existent product")
                    .Given("a product with a specific ID does not exist")
                    .WithRequest(HttpMethod.Get, $"/api/products/{productId}")
                .WillRespond()
                    .WithStatus(System.Net.HttpStatusCode.NotFound);

            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Act
                var client = new HttpClient { BaseAddress = ctx.MockServerUri };
                var response = await client.GetAsync($"/api/products/{productId}");

                // Assert
                Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            });
        }
    }
}
