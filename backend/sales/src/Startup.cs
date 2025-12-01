using System;
using System.Text;
using Azure.Monitor.OpenTelemetry.Exporter;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks; // Added for Health Checks
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NovaCommerce.Sales.Application;
using NovaCommerce.Sales.Application.Validators;
using NovaCommerce.Sales.Domain;
using NovaCommerce.Sales.Infrastructure;
using NovaCommerce.Sales.Infrastructure.Configuration;
using NovaCommerce.Sales.Infrastructure.Messaging;
using NovaCommerce.Sales.Infrastructure.Middleware;
using System.Net.Http;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Serilog;
using StackExchange.Redis;

namespace NovaCommerce.Sales
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure strongly typed options
            services.AddOptions<DatabaseOptions>()
                .Configure(options => options.ConnectionString = Configuration["SQL_CONNECTION_STRING"])
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<RabbitMQOptions>()
                .Configure(options => options.ConnectionString = Configuration["RABBITMQ_CONNECTION_STRING"])
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<JwtOptions>()
                .Configure(options => options.Secret = Configuration["JWT_SECRET"])
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<TelemetryOptions>()
                .Configure(options => options.ApplicationInsightsConnectionString = Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"])
                .ValidateOnStart();

            services.AddOptions<RedisOptions>()
                .Configure(options => options.ConnectionString = Configuration["REDIS_CONNECTION_STRING"])
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Resolve options for immediate use or injection
            var jwtOptions = services.BuildServiceProvider().GetRequiredService<IOptions<JwtOptions>>().Value;
            var databaseOptions = services.BuildServiceProvider().GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var rabbitMqOptions = services.BuildServiceProvider().GetRequiredService<IOptions<RabbitMQOptions>>().Value;
            var redisOptions = services.BuildServiceProvider().GetRequiredService<IOptions<RedisOptions>>().Value;


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddDbContext<SalesDbContext>((serviceProvider, dbContextOptions) =>
            {
                dbContextOptions.UseSqlServer(databaseOptions.ConnectionString);
            });

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<OrderService>();
            services.AddHttpClient<OrderService>();

            services.AddSingleton(provider =>
            {
                return new MessagePublisher(rabbitMqOptions.ConnectionString);
            });

            // Redis Caching
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
            });
            services.AddScoped<IOrderCacheService, RedisOrderCacheService>();

            services.AddControllers()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateOrderDtoValidator>());

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sales API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Health Checks
            services.AddHealthChecks()
                .AddSqlServer(databaseOptions.ConnectionString, name: "SQL-DB-Check", HealthStatus.Degraded, tags: new[] { "ready" })
                .AddRabbitMQ(rabbitMqOptions.ConnectionString, name: "RabbitMQ-Check", HealthStatus.Degraded, tags: new[] { "ready" })
                .AddRedis(redisOptions.ConnectionString, name: "Redis-Check", HealthStatus.Degraded, tags: new[] { "ready" });


            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("sales-service"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddConsoleExporter();

                    var telemetryOptions = services.BuildServiceProvider().GetRequiredService<IOptions<TelemetryOptions>>().Value;
                    if (!string.IsNullOrEmpty(telemetryOptions.ApplicationInsightsConnectionString))
                    {
                        tracing.AddAzureMonitorTraceExporter(o => o.ConnectionString = telemetryOptions.ApplicationInsightsConnectionString);
                    }
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sales API v1");
                });
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health"); // Basic health check
                endpoints.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("ready")
                }); // Readiness probe
            });
        }
    }
}
