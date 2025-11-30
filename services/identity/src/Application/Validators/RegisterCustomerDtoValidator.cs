using FluentValidation;
using NovaCommerce.Identity.Application.Dtos;

namespace NovaCommerce.Identity.Application.Validators
{
    public class RegisterCustomerDtoValidator : AbstractValidator<RegisterCustomerDto>
    {
        public RegisterCustomerDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
        }
    }
}
