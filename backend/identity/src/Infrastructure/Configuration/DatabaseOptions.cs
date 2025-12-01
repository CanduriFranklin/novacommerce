using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Identity.Infrastructure.Configuration
{
    public class DatabaseOptions
    {
        [Required(ErrorMessage = "SQL_CONNECTION_STRING is required.")]
        public string ConnectionString { get; set; }
    }
}
