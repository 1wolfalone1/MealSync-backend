using FluentValidation;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;

public class UpdateShopStatusInactiveActiveValidator : AbstractValidator<UpdateShopStatusInactiveActiveCommand>
{
    public UpdateShopStatusInactiveActiveValidator()
    {
        RuleFor(x => x.Status)
            .Must(x => x == (int) ShopStatus.Active || x == (int) ShopStatus.InActive)
            .WithMessage("Chỉ có thể đổi trạng thái sang hoạt động (2) hoặc ngưng hoạt động (3)");
    }
}