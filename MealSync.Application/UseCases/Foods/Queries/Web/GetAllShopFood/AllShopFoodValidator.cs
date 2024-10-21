using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.Web.GetAllShopFood;

public class AllShopFoodValidator : AbstractValidator<AllShopFoodQuery>
{
    public AllShopFoodValidator()
    {
        RuleFor(x => x.StatusMode)
            .Must(x => x == 0 || x == 1 || x == 2 || x == 3)
            .WithMessage("Vui lòng chỉ chọn 0 cho tất cả, 1 cho mở bán, 2 cho tạm hết hàng, 3 cho tạm ẩn");
    }
}