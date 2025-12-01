using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Identity.Application.Dtos
{
    public class LoginCustomerDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
