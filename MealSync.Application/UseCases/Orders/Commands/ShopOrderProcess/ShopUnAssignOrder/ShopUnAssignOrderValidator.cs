using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopUnAssignOrder;

public class ShopUnAssignOrderValidator : AbstractValidator<ShopUnAssignOrderCommand>
{
    public ShopUnAssignOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của đơn hàng");

        RuleFor(x => x.IsConfirm)
            .NotNull()
            .WithMessage("Vui lòng cung cấp là bạn cho chấp nhận bỏ quan confirm không");
    }
}