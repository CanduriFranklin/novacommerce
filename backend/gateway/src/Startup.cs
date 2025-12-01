using System.Text;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using NovaCommerce.Gateway.Infrastructure.Configuration;

namespace NovaCommerce.Services.Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Strongly typed options with fail-fast validation
            services.AddOptions<JwtOptions>()
                .Configure(o => o.Secret = Configuration["JWT_SECRET"])
                .Validate(o => !string.IsNullOrWhiteSpace(o.Secret), "JWT_SECRET is required")
                .ValidateOnStart();

            services.AddOptions<TelemetryOptions>()
                .Configure(o => o.ApplicationInsightsConnectionString = Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"])
                .ValidateOnStart();

            // JWT authentication
            var jwtSecret = Configuration["JWT_SECRET"];
            if (string.IsNullOrWhiteSpace(jwtSecret))
                throw new InvalidOperationException("JWT_SECRET is not configured.");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer("Bearer", options =>
                {
                    options.RequireHttpsMetadata = true; // enforce HTTPS in production
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                });

            // Ocelot + Polly (timeouts, retries, circuit-breakers)
            services.AddOcelot()
                    .AddPolly();

            // OpenTelemetry + Azure Monitor
            services.AddOpenTelemetry()
                .ConfigureResource(r => r.AddService("api-gateway"))
                .WithTracing(tracing =>
                {
                    tracing.AddAspNetCoreInstrumentation()
                           .AddHttpClientInstrumentation()
                           .AddConsoleExporter();

                    var aiConn = Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                    if (!string.IsNullOrWhiteSpace(aiConn))
                    {
                        tracing.AddAzureMonitorTraceExporter(o => o.ConnectionString = aiConn);
                    }
                });

            // Authorization policies (optional, recommended)
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            });

            // Security headers (defense-in-depth)
            services.AddCors(options =>
            {
                options.AddPolicy("Default", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod());
            });
        }

        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseCors("Default");

            app.UseAuthentication();
            app.UseAuthorization();

            // Ocelot must be at the end of the pipeline
            await app.UseOcelot();
        }
    }
}
