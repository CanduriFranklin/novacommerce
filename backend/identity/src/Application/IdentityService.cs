using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NovaCommerce.Identity.Application.Dtos;
using NovaCommerce.Identity.Domain;
using NovaCommerce.Identity.Infrastructure.Configuration;
using NovaCommerce.Identity.Infrastructure; // Added for IdentityDbContext
using System.Linq; // Added for ClaimsPrincipal extensions

namespace NovaCommerce.Identity.Application
{
    public class IdentityService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly JwtOptions _jwtOptions;
        private readonly IdentityDbContext _dbContext; // Injected DbContext for tracing

        public IdentityService(ICustomerRepository customerRepository, ISessionRepository sessionRepository, IOptions<JwtOptions> jwtOptions, IdentityDbContext dbContext)
        {
            _customerRepository = customerRepository;
            _sessionRepository = sessionRepository;
            _jwtOptions = jwtOptions.Value;
            _dbContext = dbContext;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterCustomerDto registerDto)
        {
            var existingCustomer = await _customerRepository.GetByEmailAsync(registerDto.Email);
            if (existingCustomer != null)
            {
                throw new ApplicationException("User with this email already exists.");
            }

            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Role = registerDto.Role // Assign role from DTO
            };

            await _customerRepository.AddAsync(customer);

            // Add trace entry
            await _dbContext.Traces.AddAsync(new Domain.Trace
            {
                AgentId = customer.CustomerId.ToString(),
                Operation = "CustomerRegistered",
                Entity = "Customer",
                EntityId = customer.CustomerId,
                Timestamp = DateTime.UtcNow,
                Details = $"Customer '{customer.Email}' registered with role '{customer.Role}'."
            });
            await _dbContext.SaveChangesAsync();

            return await GenerateJwtTokenAsync(customer);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginCustomerDto loginDto)
        {
            var customer = await _customerRepository.GetByEmailAsync(loginDto.Email);
            if (customer == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, customer.PasswordHash))
            {
                // Add trace entry for failed login attempt
                await _dbContext.Traces.AddAsync(new Domain.Trace
                {
                    AgentId = loginDto.Email, // Use email as agent for failed login
                    Operation = "LoginFailed",
                    Entity = "Customer",
                    EntityId = null,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Failed login attempt for email '{loginDto.Email}'."
                });
                await _dbContext.SaveChangesAsync();

                throw new ApplicationException("Invalid credentials.");
            }

            // Add trace entry for successful login
            await _dbContext.Traces.AddAsync(new Domain.Trace
            {
                AgentId = customer.CustomerId.ToString(),
                Operation = "LoginSuccessful",
                Entity = "Customer",
                EntityId = customer.CustomerId,
                Timestamp = DateTime.UtcNow,
                Details = $"Customer '{customer.Email}' logged in successfully with role '{customer.Role}'."
            });
            await _dbContext.SaveChangesAsync();

            return await GenerateJwtTokenAsync(customer);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string expiredToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(expiredToken);
            var customerIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(customerIdClaim, out var customerId))
            {
                throw new ApplicationException("Invalid token claims.");
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new ApplicationException("Customer not found.");
            }

            var storedSession = await _sessionRepository.GetByRefreshTokenAsync(refreshToken);

            if (storedSession == null || storedSession.CustomerId != customer.CustomerId || storedSession.IsRevoked || storedSession.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new ApplicationException("Invalid or expired refresh token.");
            }

            // Revoke the old refresh token
            storedSession.IsRevoked = true;
            await _sessionRepository.UpdateAsync(storedSession);

            // Generate new tokens
            return await GenerateJwtTokenAsync(customer);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Secret)),
                ValidateLifetime = false // We expect the token to be expired
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token.");
            }

            return principal;
        }

        private async Task<AuthResponseDto> GenerateJwtTokenAsync(Customer customer)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                    new Claim(ClaimTypes.Email, customer.Email),
                    new Claim(ClaimTypes.Role, customer.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1), // JWT valid for 1 hour
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString();
            var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Refresh token valid for 7 days

            var session = new Session
            {
                SessionId = Guid.NewGuid(),
                CustomerId = customer.CustomerId,
                JwtToken = jwtToken,
                RefreshToken = refreshToken, // Store refresh token
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = tokenDescriptor.Expires.Value,
                IsRevoked = false,
                RefreshTokenExpiryTime = refreshTokenExpiryTime // Store refresh token expiry
            };
            await _sessionRepository.AddAsync(session);

            // Add trace entry for session creation
            await _dbContext.Traces.AddAsync(new Domain.Trace
            {
                AgentId = customer.CustomerId.ToString(),
                Operation = "SessionCreated",
                Entity = "Session",
                EntityId = session.SessionId,
                Timestamp = DateTime.UtcNow,
                Details = $"Session created for customer '{customer.Email}'."
            });
            await _dbContext.SaveChangesAsync();

            return new AuthResponseDto { Token = jwtToken, RefreshToken = refreshToken };
        }
    }
}
