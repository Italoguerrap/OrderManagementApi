using FluentValidation;
using OrderManagementApi.Requests;

namespace OrderManagementApi.Validations
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("CPF é obrigatório");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Nova senha é obrigatória")
                .MinimumLength(6).WithMessage("A nova senha deve ter no mínimo 6 caracteres");
        }
    }
}