using FluentValidation;
using OrderManagement.API.Requests;

namespace OrderManagementApi.Validations
{
    public class AddProductToOrderRequestValidator : AbstractValidator<AddProductToOrderRequest>
    {
        public AddProductToOrderRequestValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
