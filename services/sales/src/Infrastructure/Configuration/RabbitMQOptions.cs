using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Sales.Infrastructure.Configuration
{
    public class RabbitMQOptions
    {
        [Required(ErrorMessage = "RABBITMQ_CONNECTION_STRING is required.")]
        public string ConnectionString { get; set; }
    }
}
