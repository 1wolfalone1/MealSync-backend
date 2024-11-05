using FluentValidation;

namespace MealSync.Application.UseCases.Reviews.Commands.ShopReplyReviewOfCustomers;

public class ShopReplyReviewOfCustomerValidator : AbstractValidator<ShopReplyReviewOfCustomerCommand>
{
    public ShopReplyReviewOfCustomerValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Id phải lớn hơn 0");

        RuleFor(x => x.Comment)
            .MaximumLength(800).WithMessage("Đánh giá tối đa 800 kí tự.");
    }
}