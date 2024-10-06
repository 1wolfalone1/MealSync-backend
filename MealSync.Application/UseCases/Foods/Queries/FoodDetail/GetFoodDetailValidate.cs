using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.FoodDetail;

public class GetFoodDetailValidate : AbstractValidator<GetFoodDetailQuery>
{
    public GetFoodDetailValidate()
    {
        RuleFor(q => q.Id)
            .GreaterThan(0)
            .WithMessage("Id thức ăn phải lớn hơn 0");
    }
}