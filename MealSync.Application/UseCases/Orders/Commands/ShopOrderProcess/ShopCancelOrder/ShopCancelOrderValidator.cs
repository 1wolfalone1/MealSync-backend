using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCancelOrder;

public class ShopCancelOrderValidator : AbstractValidator<ShopCancelOrderCommand>
{
    public ShopCancelOrderValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp lý do");

        RuleFor(x => x.IsConfirm)
            .NotNull()
            .WithMessage("Vui lòng xác nhận là có đồng ý hay không");
    }
}