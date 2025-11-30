using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace NovaCommerce.Inventory.PerformanceTests;

class Program
{
    static void Main(string[] args)
    {
        using var httpClient = new HttpClient();

        var getScenario = Scenario.Create("get_products", async context =>
        {
            var request = Http.CreateRequest("GET", "http://localhost:5001/api/products");
            var response = await Http.Send(httpClient, request);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var postScenario = Scenario.Create("post_product", async context =>
        {
            var product = new { Name = $"Perf Test Product {Guid.NewGuid()}", Price = 50.0m, Stock = 100 };
            var body = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
            var request = Http.CreateRequest("POST", "http://localhost:5001/api/products").WithBody(body);
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
