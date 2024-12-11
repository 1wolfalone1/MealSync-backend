using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Queries.OrderDetailForAdmin;

public class OrderDetailForAdminValidator : AbstractValidator<OrderDetailForAdminQuery>
{
    public OrderDetailForAdminValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng id của đơn hàng");
    }
}