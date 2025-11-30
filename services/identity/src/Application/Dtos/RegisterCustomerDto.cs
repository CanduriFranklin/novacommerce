using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Identity.Application.Dtos
{
    public class RegisterCustomerDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = "Customer"; // Allow specifying role, default to Customer
    }
}
