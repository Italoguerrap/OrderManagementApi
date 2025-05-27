using FluentValidation;
using OrderManagementApi.Requests;

namespace OrderManagementApi.Validations
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("CPF é obrigatório");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Senha é obrigatória");
        }
    }
}