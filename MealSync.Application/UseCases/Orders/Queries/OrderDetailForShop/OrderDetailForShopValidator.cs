using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Queries.OrderDetailForShop;

public class OrderDetailForShopValidator : AbstractValidator<OrderDetailForShopQuery>
{
    public OrderDetailForShopValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng id của đơn hàng");
    }
}