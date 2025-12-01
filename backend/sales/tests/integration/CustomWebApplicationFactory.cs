using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace NovaCommerce.Sales.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                var integrationConfig = new ConfigurationBuilder()
                    .AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("JWT_SECRET", "this-is-a-test-secret-key-for-jwt"),
                        new KeyValuePair<string, string>("SQL_CONNECTION_STRING", "Server=(localdb)\\\\mssqllocaldb;Database=NovaCommerce.Sales.Tests;Trusted_Connection=True;MultipleActiveResultSets=true"),
                        new KeyValuePair<string, string>("RABBITMQ_CONNECTION_STRING", "amqp://guest:guest@localhost:5672/")
                    })
                    .Build();
                config.AddConfiguration(integrationConfig);
            });
        }

        public string GenerateJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("this-is-a-test-secret-key-for-jwt");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", "1") }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
