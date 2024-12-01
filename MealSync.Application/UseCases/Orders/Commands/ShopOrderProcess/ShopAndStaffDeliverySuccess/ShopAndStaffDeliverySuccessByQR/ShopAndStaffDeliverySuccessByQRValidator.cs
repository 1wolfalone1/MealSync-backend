using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliverySuccess.ShopAndStaffDeliverySuccessByQR;

public class ShopAndStaffDeliverySuccessByQRValidator : AbstractValidator<ShopAndStaffDeliverySuccessByQRCommand>
{
    public ShopAndStaffDeliverySuccessByQRValidator()
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

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp token");
    }
}