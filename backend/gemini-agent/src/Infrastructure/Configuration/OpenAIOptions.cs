using System.ComponentModel.DataAnnotations;

namespace NovaCommerce.services.gemini_agent.Infrastructure.Configuration
{
    public class OpenAIOptions
    {
        [Required(ErrorMessage = "OPENAI_ENDPOINT is required.")]
        public string Endpoint { get; set; }

        [Required(ErrorMessage = "OPENAI_KEY1 is required.")]
        public string Key1 { get; set; }

        [Required(ErrorMessage = "OPENAI_KEY2 is required.")]
        public string Key2 { get; set; }
    }
}
