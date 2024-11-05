using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliveryFailOrder;

public class ShopDeliveryFailOrderValidator : AbstractValidator<ShopDeliveryFailOrderCommand>
{
    public ShopDeliveryFailOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của đơn hàng");

        RuleFor(x => x.ReasonIndentity)
            .Must(x => x == 1 || x == 2)
            .WithMessage("Lý do nhận định chỉ có thể là 1 (lỗi do shop) 2 (lỗi do khách hàng)");
    }
}