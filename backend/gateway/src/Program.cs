using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Ocelot.Provider.Consul; // Assuming Consul for service discovery if needed, otherwise remove.

var builder = WebApplication.CreateBuilder(args);

// Clear default configuration providers to ensure only environment variables are used
builder.Configuration.Sources.Clear();
builder.Configuration.AddEnvironmentVariables();

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Ocelot
// Ocelot will read its configuration directly from IConfiguration, which is now populated by environment variables.
// Environment variables should be structured like OCELOT__GLOBALCONFIGURATION__BASEURL, OCELOT__ROUTES__0__UPSTREAMTEMPLATE, etc.
builder.Services.AddOcelot();

// Add Authentication
var jwtSecret = builder.Configuration["AUTH__JWT_SECRET"];
var jwtIssuer = builder.Configuration["AUTH__ISSUER"];
var jwtAudience = builder.Configuration["AUTH__AUDIENCE"];

if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("AUTH__JWT_SECRET, AUTH__ISSUER, or AUTH__AUDIENCE environment variables are not set.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// Add Health Checks
// Health check URLs for downstream services should be configured via environment variables.
// Example: HEALTHCHECKS__IDENTITY__URI, HEALTHCHECKS__CATALOG__URI
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri(builder.Configuration["HEALTHCHECKS__IDENTITY__URI"] ?? throw new InvalidOperationException("HEALTHCHECKS__IDENTITY__URI is not set")),
        name: "identity-api", tags: new[] { "ready" })
    .AddUrlGroup(new Uri(builder.Configuration["HEALTHCHECKS__CATALOG__URI"] ?? throw new InvalidOperationException("HEALTHCHECKS__CATALOG__URI is not set")),
        name: "catalog-api", tags: new[] { "ready" });
    // Add other microservices health checks here dynamically based on environment variables

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map health checks
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("ready"),
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = (_) => false // Liveness check only checks if the app is running
});

await app.UseOcelot();

app.Run();
