using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShowQRConfirm;

public class ShowQRConfirmValidate : AbstractValidator<ShowQRConfirmCommand>
{
    public ShowQRConfirmValidate()
    {
        RuleFor(o => o.Id)
            .GreaterThan(0)
            .WithMessage("Order id phải lớn hơn 0");
    }
}