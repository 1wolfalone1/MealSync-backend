using FluentValidation;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Shops.Commands.ModeratorManage.UpdateShopStatus;

public class UpdateShopStatusValidate : AbstractValidator<UpdateShopStatusCommand>
{
    public UpdateShopStatusValidate()
    {
        RuleFor(q => q.Id)
            .GreaterThan(0)
            .WithMessage("Shop Id phải lớn hơn 0");

        RuleFor(x => x.Status)
            .Must(x => x == ShopStatus.InActive || x == ShopStatus.Banned)
            .WithMessage("Chỉ có thể đổi trạng thái InActive (3) hoặc Banned (5)");
    }
}