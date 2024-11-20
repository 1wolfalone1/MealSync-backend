using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Queries.FoodOfShopForModManage;

public class GetFoodOfShopForModManageValidate : AbstractValidator<GetFoodOfShopForModManageQuery>
{
    public GetFoodOfShopForModManageValidate()
    {
        RuleFor(q => q.ShopId)
            .GreaterThan(0)
            .WithMessage("Shop id phải lớn hơn 0");
    }
}