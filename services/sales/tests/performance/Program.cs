using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace NovaCommerce.Sales.PerformanceTests;

class Program
{
    static void Main(string[] args)
    {
        using var httpClient = new HttpClient();

        var getScenario = Scenario.Create("get_orders", async context =>
        {
            var request = Http.CreateRequest("GET", "http://localhost:5002/api/orders");
            var response = await Http.Send(httpClient, request);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var postScenario = Scenario.Create("post_order", async context =>
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
            var request = Http.CreateRequest("POST", "http://localhost:5002/api/orders").WithBody(body);
            var response = await Http.Send(httpClient, request);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(getScenario, postScenario)
            .Run();
    }
}
