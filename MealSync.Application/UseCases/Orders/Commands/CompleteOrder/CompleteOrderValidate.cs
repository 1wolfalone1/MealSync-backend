using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.CompleteOrder;

public class CompleteOrderValidate : AbstractValidator<CompleteOrderCommand>
{
    public CompleteOrderValidate()
    {
        RuleFor(o => o.Id)
            .GreaterThan(0)
            .WithMessage("Order id phải lớn hơn 0");
    }
}