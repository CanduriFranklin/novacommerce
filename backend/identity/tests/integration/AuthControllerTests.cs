using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NovaCommerce.Identity.Application.Dtos;
using Xunit;

namespace NovaCommerce.Identity.IntegrationTests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOk()
        {
            // Arrange
            var registerDto = new RegisterCustomerDto
            {
                Name = "Integration Test User",
                Email = $"test.integration.{Guid.NewGuid()}@example.com",
                Password = "Password123!"
            };
            var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/register", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(authResponse.Token);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var email = $"duplicate.integration.{Guid.NewGuid()}@example.com";
            var registerDto1 = new RegisterCustomerDto
            {
                Name = "User One",
                Email = email,
                Password = "Password123!"
            };
            var content1 = new StringContent(JsonSerializer.Serialize(registerDto1), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/v1/auth/register", content1); // Register first user

            var registerDto2 = new RegisterCustomerDto
            {
                Name = "User Two",
                Email = email,
                Password = "Password123!"
            };
            var content2 = new StringContent(JsonSerializer.Serialize(registerDto2), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/register", content2);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // Arrange
            var email = $"login.integration.{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var registerDto = new RegisterCustomerDto
            {
                Name = "Login Test User",
                Email = email,
                Password = password
            };
            var registerContent = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/v1/auth/register", registerContent); // Register user

            var loginDto = new LoginCustomerDto
            {
                Email = email,
                Password = password
            };
            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", loginContent);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(authResponse.Token);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginCustomerDto
            {
                Email = $"nonexistent.{Guid.NewGuid()}@example.com",
                Password = "WrongPassword!"
            };
            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", loginContent);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
