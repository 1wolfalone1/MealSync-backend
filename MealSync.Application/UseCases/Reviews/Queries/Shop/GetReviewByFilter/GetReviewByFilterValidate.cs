using FluentValidation;

namespace MealSync.Application.UseCases.Reviews.Queries.Shop.GetReviewByFilter;

public class GetReviewByFilterValidate : AbstractValidator<GetReviewByFilterQuery>
{
    public GetReviewByFilterValidate()
    {
        RuleFor(x => x.Filter)
            .IsInEnum().WithMessage("1: Tất cả, 2: Chỉ có comment, 3: Có comment và ảnh");
    }
}