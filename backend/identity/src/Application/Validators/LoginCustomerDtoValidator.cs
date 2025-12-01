using FluentValidation;
using NovaCommerce.Identity.Application.Dtos;

namespace NovaCommerce.Identity.Application.Validators
{
    public class LoginCustomerDtoValidator : AbstractValidator<LoginCustomerDto>
    {
        public LoginCustomerDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
