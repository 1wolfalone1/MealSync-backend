using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliverySuccess;

public class ShopDeliverySuccessValidator : AbstractValidator<ShopDeliverySuccessCommand>
{
    public ShopDeliverySuccessValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của đơn hàng");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của khách hàng");

        RuleFor(x => x.ShipperId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id người giao hàng");

        RuleFor(x => x.OrderDate)
            .NotNull()
            .WithMessage("Vui lòng cung cấp ngày đặt hàng");
    }
}