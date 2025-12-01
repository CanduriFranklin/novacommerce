using System;
using NovaCommerce.Identity.Domain;
using Xunit;

namespace NovaCommerce.Identity.UnitTests
{
    public class CustomerTests
    {
        [Fact]
        public void Customer_Creation_Should_Set_Properties_Correctly()
        {
            // Arrange
            var name = "Test Customer";
            var email = "test@example.com";
            var passwordHash = "hashedpassword";

            // Act
            var customer = new Customer
            {
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Assert
            Assert.Equal(name, customer.Name);
            Assert.Equal(email, customer.Email);
            Assert.Equal(passwordHash, customer.PasswordHash);
            Assert.True(customer.IsActive);
            Assert.NotEqual(Guid.Empty, customer.CustomerId);
        }
    }
}
