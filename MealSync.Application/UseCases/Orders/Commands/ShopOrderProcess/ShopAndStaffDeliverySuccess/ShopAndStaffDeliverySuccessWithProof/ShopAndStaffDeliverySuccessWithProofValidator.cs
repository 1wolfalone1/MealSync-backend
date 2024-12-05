namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliverySuccessWithProof;

using FluentValidation;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliverySuccess.ShopAndStaffDeliverySuccessWithProof;

public class ShopAndStaffDeliverySuccessWithProofValidator : AbstractValidator<ShopAndStaffDeliverySuccessWithProofCommand>
{
    public ShopAndStaffDeliverySuccessWithProofValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp order id");

        RuleFor(x => x.ImageProofs)
            .Must(x => x != null && x.Length > 0)
            .WithMessage("Vui lòng cung cấp ít nhất 1 ảnh làm bằng chứng giao hàng thành công");
    }
}