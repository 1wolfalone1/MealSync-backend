using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopConfirmListOrder;

public class ShopConfirmListOrderValidator : AbstractValidator<ShopConfirmListOrderCommand>
{
    public ShopConfirmListOrderValidator()
    {
        RuleFor(x => x.Ids)
            .Must(x => x != null && x.Length > 0)
            .WithMessage("Vui lòng cung cấp id đơn hàng");
    }
}