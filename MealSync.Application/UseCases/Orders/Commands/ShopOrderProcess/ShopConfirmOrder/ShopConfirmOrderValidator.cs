using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopConfirmOrder;

public class ShopConfirmOrderValidator : AbstractValidator<ShopConfirmOrderCommand>
{
    public ShopConfirmOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id đơn hàng");
    }
}