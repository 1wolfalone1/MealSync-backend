using FluentValidation;

namespace MealSync.Application.UseCases.Promotions.Queries.Shop.GetPromotionDetail;

public class GetPromotionDetailValidate : AbstractValidator<GetPromotionDetailQuery>
{
    public GetPromotionDetailValidate()
    {
        RuleFor(q => q.Id)
            .GreaterThan(0)
            .WithMessage("Id phải lớn hơn 0");
    }
}