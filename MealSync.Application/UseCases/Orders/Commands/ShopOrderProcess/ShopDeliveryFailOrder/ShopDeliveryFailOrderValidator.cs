using FluentValidation;
using MealSync.Application.Common.Utils;
using MealSync.Domain.Entities;

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

        RuleForEach(x => x.Evidences)
            .SetValidator(new ShopDeliveryEvidenceValidator());
    }
}

public class ShopDeliveryEvidenceValidator : AbstractValidator<ShopDeliveyFailEvidence>
{
    public ShopDeliveryEvidenceValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ảnh");

        RuleFor(x => x.TakePictureDateTime)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ngày chụp ảnh ")
            .Must(x => x <= TimeFrameUtils.GetCurrentDate())
            .WithMessage("Thời gian chụp ảnh không thể lớn hơn thời gian hiện tại. Note: Cung cấp thời gian UTC");
    }
}