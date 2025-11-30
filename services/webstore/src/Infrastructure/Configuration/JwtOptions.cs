using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Webstore.Infrastructure.Configuration
{
    public class JwtOptions
    {
        [Required(ErrorMessage = "JWT_SECRET is required.")]
        public string Secret { get; set; }
    }
}
