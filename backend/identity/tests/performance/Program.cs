using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace NovaCommerce.Identity.PerformanceTests;

class Program
{
    static void Main(string[] args)
    {
        using var httpClient = new HttpClient();

        var registerScenario = Scenario.Create("register_user", async context =>
        {
            var userEmail = $"perf.user.{Guid.NewGuid()}@example.com";
            var userPassword = "Password123!";
            var registerDto = new
            {
                Name = $"Perf User {Guid.NewGuid()}",
                Email = userEmail,
                Password = userPassword,
                Role = "Customer"
            };
            var body = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
            var request = Http.CreateRequest("POST", "http://localhost:5003/api/v1/auth/register").WithBody(body);
            var response = await Http.Send(httpClient, request);

            // Store credentials for potential subsequent login attempts if needed in a more complex flow
            // For this simple scenario, we just check registration success
            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var loginScenario = Scenario.Create("login_user", async context =>
        {
            // For a more realistic login test, you'd pre-register users and pick from a pool.
            // Here, we'll simulate unique login attempts, assuming they might fail if not registered.
            // Or, you could chain this after a successful registration in a single user journey.
            var userEmail = $"login.perf.user.{Guid.NewGuid()}@example.com"; // Unique email for each attempt
            var userPassword = "Password123!"; // Consistent password for simplicity

            var loginDto = new
            {
                Email = userEmail,
                Password = userPassword
            };
            var body = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
            var request = Http.CreateRequest("POST", "http://localhost:5003/api/v1/auth/login").WithBody(body);
            var response = await Http.Send(httpClient, request);

            // Expecting Unauthorized for non-existent users, but Ok for successful logins
            return response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        NBomberRunner
            .RegisterScenarios(registerScenario, loginScenario)
            .Run();
    }
}
