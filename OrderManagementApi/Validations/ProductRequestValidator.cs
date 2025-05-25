using FluentValidation;
using OrderManagementApi.Requests;

namespace OrderManagementApi.Validations
{
    public class ProductRequestValidator : AbstractValidator<ProductRequest>
    {
        public ProductRequestValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("O nome do produto é obrigatório")
                .MaximumLength(100).WithMessage("O nome do produto deve ter no máximo 100 caracteres");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("O preço deve ser maior que zero");
        }
    }
}