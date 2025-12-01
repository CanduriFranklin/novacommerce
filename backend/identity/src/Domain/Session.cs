using System;
using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.Identity.Domain
{
    public class Session
    {
        [Key]
        public Guid SessionId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public string JwtToken { get; set; }

        public DateTime IssuedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
