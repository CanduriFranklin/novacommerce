using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NovaCommerce.SupportAgent.Application;
using NovaCommerce.SupportAgent.Application.Dtos;

namespace NovaCommerce.SupportAgent.Api.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SupportController : ControllerBase
    {
        private readonly SupportService _supportService;

        public SupportController(SupportService supportService)
        {
            _supportService = supportService;
        }

        [HttpPost("query")]
        public async Task<ActionResult<string>> ProcessQuery(QueryDto queryDto)
        {
            var response = await _supportService.ProcessQueryAsync(queryDto.Query);
            return Ok(response);
        }
    }
}
