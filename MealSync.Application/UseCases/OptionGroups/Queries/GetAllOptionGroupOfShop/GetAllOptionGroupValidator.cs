using FluentValidation;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetAllOptionGroupOfShop;

public class GetAllOptionGroupValidator : AbstractValidator<GetAllShopOptionGroupQuery>
{
    public GetAllOptionGroupValidator()
    {
        RuleFor(x => x.Status)
            .Must(x => x >= 0 && x <= 2)
            .WithMessage("Status 0 cho tất cả, 1 cho hoạt động, 2 cho tạm ngưng");
    }
}