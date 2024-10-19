using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopRejectOrder;

public class ShopRejectOrderValidate : AbstractValidator<ShopRejectOrderCommand>
{
    public ShopRejectOrderValidate()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id");

        RuleFor(x => x.Reason)
            .Must(x => !string.IsNullOrEmpty(x))
            .WithMessage("Vui lòng cung cấp lý do");
    }
}