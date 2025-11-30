using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NovaCommerce.Sales.Domain;
using Xunit;

namespace NovaCommerce.Sales.IntegrationTests
{
    public class OrderApiTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public OrderApiTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_Orders_With_Invalid_Token_Should_Return_Unauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/orders");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_Order_By_Id_With_Non_Existent_Id_Should_Return_NotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

            // Act
            var response = await _client.GetAsync($"/api/orders/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_Order_With_Valid_Token_Should_Create_And_Return_Order()
        {
            // Arrange
            var order = new Order
            {
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.0m }
                }
            };
            var content = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

            // Act
            var response = await _client.PostAsync("/api/orders", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var createdOrder = JsonSerializer.Deserialize<Order>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Equal(order.CustomerId, createdOrder.CustomerId);
        }
    }
}
