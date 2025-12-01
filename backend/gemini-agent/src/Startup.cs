using System;
using Azure.Monitor.OpenTelemetry.Exporter;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NovaCommerce.services.gemini_agent.Infrastructure.Configuration;
using NovaCommerce.SupportAgent.Application;
using NovaCommerce.SupportAgent.Application.Validators;
using NovaCommerce.SupportAgent.Infrastructure.Configuration;
using NovaCommerce.SupportAgent.Infrastructure.Middleware;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Serilog;

namespace NovaCommerce.SupportAgent
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
            services.AddOptions<OpenAIOptions>()
                .Configure(options =>
                {
                    options.Endpoint = Configuration["OPENAI_ENDPOINT"];
                    options.Key1 = Configuration["OPENAI_KEY1"];
                    options.Key2 = Configuration["OPENAI_KEY2"];
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<TelemetryOptions>()
                .Configure(options => options.ApplicationInsightsConnectionString = Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"])
                .ValidateOnStart();

            services.AddScoped<SupportService>();

            services.AddControllers()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<QueryDtoValidator>());

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Support Agent API", Version = "v1" });
            });

            services.AddHealthChecks();

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("support-agent-service"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Support Agent API v1");
                });
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
