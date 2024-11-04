using FluentValidation;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;

public class UpdateShopStatusInactiveActiveValidator : AbstractValidator<UpdateShopStatusInactiveActiveCommand>
{
    public UpdateShopStatusInactiveActiveValidator()
    {
        RuleFor(x => x.Status)
            .Must(x => x == ShopStatus.Active || x == ShopStatus.InActive)
            .WithMessage("Chỉ có thể đổi trạng thái sang hoạt động (2) hoặc ngưng hoạt động (3)");

        When(x => x.IsReceivingOrderPaused, () =>
        {
            RuleFor(x => x.Status)
                .Must(x => x == ShopStatus.Active)
                .WithMessage("Cửa hàng đang tạm ngưng hoạt động bạn không cần thực hiện thao tác này");
        });
    }
}