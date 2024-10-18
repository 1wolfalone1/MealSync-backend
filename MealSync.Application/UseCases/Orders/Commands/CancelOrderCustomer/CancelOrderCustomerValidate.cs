using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.CancelOrderCustomer;

public class CancelOrderCustomerValidate : AbstractValidator<CancelOrderCustomerCommand>
{
    public CancelOrderCustomerValidate()
    {
        RuleFor(o => o.Id)
            .GreaterThan(0)
            .WithMessage("Order id phải lớn hơn 0");
    }
}