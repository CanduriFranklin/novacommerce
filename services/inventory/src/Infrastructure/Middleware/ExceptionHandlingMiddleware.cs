using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace NovaCommerce.Inventory.Infrastructure.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";
            var details = exception.Message;

            switch (exception)
            {
                case ApplicationException appEx:
                    statusCode = HttpStatusCode.BadRequest; // Or Conflict, depending on the specific ApplicationException
                    message = appEx.Message;
                    break;
                case ValidationException validationEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Validation failed.";
                    details = JsonSerializer.Serialize(validationEx.Errors);
                    break;
                // Add more specific exception handling here if needed
                default:
                    // Log the full exception details for unhandled exceptions
                    Log.Error(exception, "An unhandled exception occurred: {ErrorMessage}", exception.Message);
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            var response = new
            {
                StatusCode = (int)statusCode,
                Message = message,
                Details = details
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
