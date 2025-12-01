using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Inventory.Infrastructure.Configuration
{
    public class JwtOptions
    {
        [Required(ErrorMessage = "JWT_SECRET is required.")]
        public string Secret { get; set; }
    }
}
