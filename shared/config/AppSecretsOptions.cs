using System;

namespace NovaCommerce.Shared.Config
{
    /// <summary>
    /// Strongly typed container for environment-sourced secrets and connection strings.
    /// Values are populated from process environment variables only (no files).
    /// </summary>
    public sealed class AppSecretsOptions
    {
        // Authentication and security
        public string? JwtSecret { get; set; }
        public string? OAuthClientSecret { get; set; }
        public string? ApiKey { get; set; }

        // Persistence and caching
        public string? SqlConnectionString { get; set; }
        public string? RedisConnectionString { get; set; }

        // Image storage
        public string? StorageConnectionString { get; set; }

        // Auxiliary services
        public string? OpenAiEndpoint { get; set; }
        public string? OpenAiKey1 { get; set; }
        public string? OpenAiKey2 { get; set; }

        // Messaging (declared in instructions for RabbitMQ consumption)
        public string? RabbitMqConnectionString { get; set; }
    }
}
