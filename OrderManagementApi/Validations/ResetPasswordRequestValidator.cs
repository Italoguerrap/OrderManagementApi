using FluentValidation;
using OrderManagementApi.Requests;

namespace OrderManagementApi.Validations
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("CPF � obrigat�rio");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Nova senha � obrigat�ria")
                .MinimumLength(6).WithMessage("A nova senha deve ter no m�nimo 6 caracteres");
        }
    }
}