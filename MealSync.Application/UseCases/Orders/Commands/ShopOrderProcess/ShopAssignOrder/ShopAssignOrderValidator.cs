using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliveringOrder;

public class ShopAssignOrderValidator : AbstractValidator<ShopAssignOrderCommand>
{
    public ShopAssignOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của đơn hàng");

        RuleFor(x => x.IsConfirm)
            .NotNull()
            .WithMessage("Vui lòng cung cấp là bạn cho chấp nhận bỏ quan confirm không");

        RuleFor(x => x.ShopDeliveryStaffId)
            .GreaterThan(0)
            .WithMessage("Vui lòng cung cấp id của người giao hàng")
            .When(x => x.ShopDeliveryStaffId != null);
    }
}