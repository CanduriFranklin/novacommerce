using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NovaCommerce.services.tests.e2e
{
    public class FullWorkflowTests
    {
        private readonly HttpClient _client;
        private string _jwtToken;

        public FullWorkflowTests()
        {
            _client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        }

        private async Task AuthenticateClient(string role = "Customer")
        {
            // Register a new user with the specified role
            var registerDto = new
            {
                Name = $"E2E User {Guid.NewGuid()}",
                Email = $"e2e.user.{Guid.NewGuid()}@example.com",
                Password = "Password123!",
                Role = role // Set the role dynamically
            };
            var registerContent = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
            var registerResponse = await _client.PostAsync("/api/v1/auth/register", registerContent);
            registerResponse.EnsureSuccessStatusCode();

            // Login the user to get a token
            var loginDto = new
            {
                Email = registerDto.Email,
                Password = registerDto.Password
            };
            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/v1/auth/login", loginContent);
            loginResponse.EnsureSuccessStatusCode();

            var authResponse = JsonSerializer.Deserialize<JsonElement>(await loginResponse.Content.ReadAsStringAsync());
            _jwtToken = authResponse.GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        [Fact]
        public async Task CreateProductAndOrder_Should_Succeed()
        {
            await AuthenticateClient("Admin"); // Authenticate as Admin to create product

            // Arrange: Create a product
            var product = new { Name = "E2E Test Product", Price = 30.0m, Stock = 20 };
            var productContent = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
            var productResponse = await _client.PostAsync("/api/v1/inventory/products", productContent);
            productResponse.EnsureSuccessStatusCode();
            var productResponseString = await productResponse.Content.ReadAsStringAsync();
            var createdProduct = JsonSerializer.Deserialize<JsonElement>(productResponseString);
            var productId = createdProduct.GetProperty("productId").GetGuid();

            // Re-authenticate as Customer to create order
            await AuthenticateClient("Customer");

            // Act: Create an order for the product
            var order = new
            {
                CustomerId = Guid.NewGuid(), // This CustomerId is for the order, not the authenticated user
                OrderItems = new[]
                {
                    new { ProductId = productId, Quantity = 1, UnitPrice = 30.0m }
                }
            };
            var orderContent = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
            var orderResponse = await _client.PostAsync("/api/v1/sales/orders", orderContent);

            // Assert
            orderResponse.EnsureSuccessStatusCode();
            var orderResponseString = await orderResponse.Content.ReadAsStringAsync();
            var createdOrder = JsonSerializer.Deserialize<JsonElement>(orderResponseString);
            Assert.Equal(order.CustomerId, createdOrder.GetProperty("customerId").GetGuid());
        }

        [Fact]
        public async Task ConfirmOrder_Should_Update_Stock()
        {
            await AuthenticateClient("Admin"); // Authenticate as Admin to create product and confirm order

            // Arrange: Create a product
            var initialStock = 50;
            var product = new { Name = "E2E Stock Test Product", Price = 40.0m, Stock = initialStock };
            var productContent = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
            var productResponse = await _client.PostAsync("/api/v1/inventory/products", productContent);
            productResponse.EnsureSuccessStatusCode();
            var productResponseString = await productResponse.Content.ReadAsStringAsync();
            var createdProduct = JsonSerializer.Deserialize<JsonElement>(productResponseString);
            var productId = createdProduct.GetProperty("productId").GetGuid();

            // Re-authenticate as Customer to create order
            await AuthenticateClient("Customer");

            // Arrange: Create an order
            var orderQuantity = 5;
            var order = new
            {
                CustomerId = Guid.NewGuid(),
                OrderItems = new[]
                {
                    new { ProductId = productId, Quantity = orderQuantity, UnitPrice = 40.0m }
                }
            };
            var orderContent = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
            var orderResponse = await _client.PostAsync("/api/v1/sales/orders", orderContent);
            orderResponse.EnsureSuccessStatusCode();
            var orderResponseString = await orderResponse.Content.ReadAsStringAsync();
            var createdOrder = JsonSerializer.Deserialize<JsonElement>(orderResponseString);
            var orderId = createdOrder.GetProperty("orderId").GetGuid();

            // Re-authenticate as Admin to confirm order
            await AuthenticateClient("Admin");

            // Act: Confirm the order
            var confirmResponse = await _client.PostAsync($"/api/v1/sales/orders/{orderId}/confirm", null);
            confirmResponse.EnsureSuccessStatusCode();

            // Assert: Verify stock is updated
            await Task.Delay(5000); // Allow time for the message to be processed
            var updatedProductResponse = await _client.GetAsync($"/api/v1/inventory/products/{productId}");
            updatedProductResponse.EnsureSuccessStatusCode();
            var updatedProductResponseString = await updatedProductResponse.Content.ReadAsStringAsync();
            var updatedProduct = JsonSerializer.Deserialize<JsonElement>(updatedProductResponseString);
            var updatedStock = updatedProduct.GetProperty("stock").GetInt32();
            Assert.Equal(initialStock - orderQuantity, updatedStock);
        }

        [Fact]
        public async Task CreateProduct_WithoutAdminRole_ShouldReturnForbidden()
        {
            await AuthenticateClient("Customer"); // Authenticate as a regular customer

            // Arrange: Attempt to create a product (Admin-only operation)
            var product = new { Name = "Unauthorized Product", Price = 10.0m, Stock = 10 };
            var productContent = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/inventory/products", productContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
        {
            // Clear any existing authorization header
            _client.DefaultRequestHeaders.Authorization = null;

            // Arrange: Attempt to access a protected endpoint without any token
            var response = await _client.GetAsync("/api/v1/inventory/products");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
