using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.GetAll;

public class GetAllShopFoodValidate : AbstractValidator<GetAllShopFoodQuery>
{
    public GetAllShopFoodValidate()
    {
        RuleFor(q => q.ShopId)
            .GreaterThan(0)
            .WithMessage("Shop id phải lớn hơn 0");
    }
}