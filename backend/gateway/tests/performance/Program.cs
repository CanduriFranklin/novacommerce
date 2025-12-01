using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace NovaCommerce.Gateway.PerformanceTests;

class Program
{
    private static string _jwtToken;
    private static readonly HttpClient _authClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

    static async Task Main(string[] args)
    {
        // Pre-authenticate a user to get a token for subsequent requests
        await AuthenticateUser();

        var authenticatedProductRetrieval = Scenario.Create("authenticated_product_retrieval", async context =>
        {
            var request = Http.CreateRequest("GET", "http://localhost:5000/api/v1/inventory/products")
                              .WithHeader("Authorization", $"Bearer {_jwtToken}");
            var response = await Http.Send(_authClient, request);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var authenticatedOrderCreation = Scenario.Create("authenticated_order_creation", async context =>
        {
            var order = new
            {
                CustomerId = Guid.NewGuid(),
                OrderItems = new[]
                {
                    new { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.0m }
                }
            };
            var body = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
            var request = Http.CreateRequest("POST", "http://localhost:5000/api/v1/sales/orders")
                              .WithBody(body)
                              .WithHeader("Authorization", $"Bearer {_jwtToken}");
            var response = await Http.Send(_authClient, request);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        NBomberRunner
            .RegisterScenarios(authenticatedProductRetrieval, authenticatedOrderCreation)
            .Run();
    }

    private static async Task AuthenticateUser()
    {
        // Register a new user
        var registerDto = new
        {
            Name = $"Perf Gateway User {Guid.NewGuid()}",
            Email = $"perf.gateway.{Guid.NewGuid()}@example.com",
            Password = "Password123!",
            Role = "Customer"
        };
        var registerContent = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
        var registerResponse = await _authClient.PostAsync("/api/v1/auth/register", registerContent);
        registerResponse.EnsureSuccessStatusCode();

        // Login the user to get a token
        var loginDto = new
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        };
        var loginContent = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        var loginResponse = await _authClient.PostAsync("/api/v1/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var authResponse = JsonSerializer.Deserialize<JsonElement>(await loginResponse.Content.ReadAsStringAsync());
        _jwtToken = authResponse.GetProperty("token").GetString();
    }
}
