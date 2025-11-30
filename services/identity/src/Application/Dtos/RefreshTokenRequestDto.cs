using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Identity.Application.Dtos
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string ExpiredToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
