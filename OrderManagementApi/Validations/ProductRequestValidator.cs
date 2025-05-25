using FluentValidation;
using OrderManagementApi.Requests;

namespace OrderManagementApi.Validations
{
    public class ProductRequestValidator : AbstractValidator<ProductRequest>
    {
        public ProductRequestValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("O nome do produto � obrigat�rio")
                .MaximumLength(100).WithMessage("O nome do produto deve ter no m�ximo 100 caracteres");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("O pre�o deve ser maior que zero");
        }
    }
}