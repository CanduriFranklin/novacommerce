using System;
using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Identity.Domain
{
    public class Customer
    {
        [Key]
        public Guid CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Customer"; // Default role
    }
}
