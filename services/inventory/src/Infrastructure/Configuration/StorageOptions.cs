using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Inventory.Infrastructure.Configuration
{
    public class StorageOptions
    {
        [Required(ErrorMessage = "STORAGE_CONNECTION_STRING is required.")]
        public string ConnectionString { get; set; }
    }
}
