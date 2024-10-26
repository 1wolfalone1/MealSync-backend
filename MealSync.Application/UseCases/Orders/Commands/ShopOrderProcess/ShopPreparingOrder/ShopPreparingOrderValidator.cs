using System.Data;
using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopPreparingOrder;

public class ShopPreparingOrderValidator : AbstractValidator<ShopPreparingOrderCommand>
{
    public ShopPreparingOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của đơn hàng");

        RuleFor(x => x.IsConfirm)
            .NotNull()
            .WithMessage("Vui lòng cung cấp bạn có chấp nhận cảnh báo không");
    }
}