using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.SupportAgent.Application.Dtos
{
    public class QueryDto
    {
        [Required]
        public string Query { get; set; }
    }
}
