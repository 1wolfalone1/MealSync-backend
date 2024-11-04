using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliveringOrders;

public class ShopAndStaffDeliveringOrderValidator : AbstractValidator<ShopAndStaffDeliveringOrderCommand>
{
    public ShopAndStaffDeliveringOrderValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ít nhất 1 id của đơn hàng")
            .ForEach(id => id
                .GreaterThan(0)
                .WithMessage("Vui lòng cung cấp status từ 1-3"));
    }
}