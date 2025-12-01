using FluentValidation;
using NovaCommerce.SupportAgent.Application.Dtos;

namespace NovaCommerce.SupportAgent.Application.Validators
{
    public class QueryDtoValidator : AbstractValidator<QueryDto>
    {
        public QueryDtoValidator()
        {
            RuleFor(x => x.Query).NotEmpty().MaximumLength(500);
        }
    }
}
