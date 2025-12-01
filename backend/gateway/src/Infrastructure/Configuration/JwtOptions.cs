using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Gateway.Infrastructure.Configuration
{
    public class JwtOptions
    {
        [Required(ErrorMessage = "JWT_SECRET is required.")]
        public string Secret { get; set; }
    }
}
