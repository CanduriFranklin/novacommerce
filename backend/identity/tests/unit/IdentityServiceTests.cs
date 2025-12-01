using System;
using System.Threading.Tasks;
using Moq;
using NovaCommerce.Identity.Application;
using NovaCommerce.Identity.Application.Dtos;
using NovaCommerce.Identity.Domain;
using NovaCommerce.Identity.Infrastructure.Configuration;
using Xunit;
using Microsoft.Extensions.Options;

namespace NovaCommerce.Identity.UnitTests
{
    public class IdentityServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<ISessionRepository> _sessionRepositoryMock;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly IdentityService _identityService;

        public IdentityServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            _jwtOptions = Options.Create(new JwtOptions { Secret = "this-is-a-very-long-and-secure-secret-key-for-jwt-testing" });
            _identityService = new IdentityService(_customerRepositoryMock.Object, _sessionRepositoryMock.Object, _jwtOptions);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateNewCustomerAndReturnToken()
        {
            // Arrange
            var registerDto = new RegisterCustomerDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!"
            };
            _customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(registerDto.Email)).ReturnsAsync((Customer)null);

            // Act
            var result = await _identityService.RegisterAsync(registerDto);

            // Assert
            _customerRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Customer>()), Times.Once);
            _sessionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Session>()), Times.Once);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
        {
            // Arrange
            var registerDto = new RegisterCustomerDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!"
            };
            _customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(registerDto.Email)).ReturnsAsync(new Customer());

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _identityService.RegisterAsync(registerDto));
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var loginDto = new LoginCustomerDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = loginDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password)
            };
            _customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(loginDto.Email)).ReturnsAsync(customer);

            // Act
            var result = await _identityService.LoginAsync(loginDto);

            // Assert
            _sessionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Session>()), Times.Once);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var loginDto = new LoginCustomerDto
            {
                Email = "test@example.com",
                Password = "WrongPassword!"
            };
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = loginDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            };
            _customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(loginDto.Email)).ReturnsAsync(customer);

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _identityService.LoginAsync(loginDto));
        }
    }
}
