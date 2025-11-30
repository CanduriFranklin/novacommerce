using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Inventory.Infrastructure.Configuration
{
    public class RedisOptions
    {
        [Required(ErrorMessage = "REDIS_CONNECTION_STRING is required.")]
        public string ConnectionString { get; set; }
    }
}
