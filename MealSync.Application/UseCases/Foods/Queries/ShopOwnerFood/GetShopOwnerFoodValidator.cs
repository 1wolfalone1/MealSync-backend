using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.ShopOwnerFood;

public class GetShopOwnerFoodValidator : AbstractValidator<GetShopOwnerFoodQuery>
{
    public GetShopOwnerFoodValidator()
    {
        RuleFor(x => x.FilterMode)
            .Must(x => x >= 0 && x <= 5)
            .WithMessage("Chỉ cung cấp 6 cấp độ 0 - 5 tìm kiếm");
    }
}