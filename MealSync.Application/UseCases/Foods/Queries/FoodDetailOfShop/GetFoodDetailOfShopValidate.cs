using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.FoodDetailOfShop;

public class GetFoodDetailOfShopValidate : AbstractValidator<GetFoodDetailOfShopQuery>
{
    public GetFoodDetailOfShopValidate()
    {
        RuleFor(q => q.Id)
            .GreaterThan(0)
            .WithMessage("Id thức ăn phải lớn hơn 0");
    }
}