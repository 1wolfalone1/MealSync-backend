using FluentValidation;
using MealSync.Application.UseCases.Reviews.Queries.GetShopReviewByOrderId;

namespace MealSync.Application.UseCases.Reviews.Queries.GetOrderByOrderId;

public class GetShopReviewByOrderIdValidator : AbstractValidator<GetShopReviewByOrderIdQuery>
{
    public GetShopReviewByOrderIdValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id");
    }
}