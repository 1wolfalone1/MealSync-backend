using FluentValidation;

namespace MealSync.Application.UseCases.Reviews.Queries.GetReviewOfShop;

public class GetReviewOfShopValidate : AbstractValidator<GetReviewOfShopQuery>
{
    public GetReviewOfShopValidate()
    {
        RuleFor(x => x.ShopId)
            .GreaterThan(0).WithMessage("ShopId phải lớn hơn 0");

        RuleFor(x => x.Filter)
            .IsInEnum().WithMessage("1: Tất cả, 2: Chỉ có comment, 3: Có comment và ảnh");
    }
}