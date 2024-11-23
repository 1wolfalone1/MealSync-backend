using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Queries.ModeratorManage.GetOrderDetailForModerator;

public class GetOrderDetailForModeratorValidator : AbstractValidator<GetOrderDetailForModeratorQuery>
{
    public GetOrderDetailForModeratorValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của đơn hàng");
    }
}