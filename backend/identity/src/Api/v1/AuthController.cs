using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NovaCommerce.Identity.Application;
using NovaCommerce.Identity.Application.Dtos;

namespace NovaCommerce.Identity.Api.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IdentityService _identityService;

        public AuthController(IdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterCustomerDto registerDto)
        {
            try
            {
                var response = await _identityService.RegisterAsync(registerDto);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginCustomerDto loginDto)
        {
            try
            {
                var response = await _identityService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            try
            {
                var response = await _identityService.RefreshTokenAsync(refreshTokenRequestDto.ExpiredToken, refreshTokenRequestDto.RefreshToken);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
