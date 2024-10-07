using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.FoodDetail;

public class GetFoodDetailValidate : AbstractValidator<GetFoodDetailQuery>
{
    public GetFoodDetailValidate()
    {
        RuleFor(q => q.FoodId)
            .GreaterThan(0)
            .WithMessage("Id thức ăn phải lớn hơn 0");

        RuleFor(q => q.ShopId)
            .GreaterThan(0)
            .WithMessage("Id cửa hàng phải lớn hơn 0");
    }
}