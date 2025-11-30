using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NovaCommerce.Inventory.Domain;
using Xunit;

namespace NovaCommerce.Inventory.IntegrationTests
{
    public class ProductApiTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public ProductApiTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_Products_With_Invalid_Token_Should_Return_Unauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_Product_By_Id_With_Non_Existent_Id_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

            // Act
            var response = await _client.GetAsync($"/api/products/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_Product_With_Valid_Token_Should_Create_And_Return_Product()
        {
            // Arrange
            var product = new Product
            {
                Name = "Integration Test Product",
                Price = 20.0m,
                Stock = 50
            };
            var content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");


            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var createdProduct = JsonSerializer.Deserialize<Product>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Equal(product.Name, createdProduct.Name);
        }
    }
}
