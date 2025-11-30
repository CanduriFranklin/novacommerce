using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Inventory.Infrastructure.Configuration
{
    public class RabbitMQOptions
    {
        [Required(ErrorMessage = "RABBITMQ_CONNECTION_STRING is required.")]
        public string ConnectionString { get; set; }
    }
}
