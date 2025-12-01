using System;
using NovaCommerce.Identity.Domain;
using Xunit;

namespace NovaCommerce.Identity.UnitTests
{
    public class SessionTests
    {
        [Fact]
        public void Session_Creation_Should_Set_Properties_Correctly()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var jwtToken = "some.jwt.token";
            var issuedAt = DateTime.UtcNow;
            var expiresAt = DateTime.UtcNow.AddHours(1);

            // Act
            var session = new Session
            {
                CustomerId = customerId,
                JwtToken = jwtToken,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt,
                IsRevoked = false
            };

            // Assert
            Assert.Equal(customerId, session.CustomerId);
            Assert.Equal(jwtToken, session.JwtToken);
            Assert.Equal(issuedAt, session.IssuedAt);
            Assert.Equal(expiresAt, session.ExpiresAt);
            Assert.False(session.IsRevoked);
            Assert.NotEqual(Guid.Empty, session.SessionId);
        }
    }
}
