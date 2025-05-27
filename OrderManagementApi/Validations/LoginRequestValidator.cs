using FluentValidation;
using OrderManagementApi.Requests;

namespace OrderManagementApi.Validations
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("CPF � obrigat�rio");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Senha � obrigat�ria");
        }
    }
}