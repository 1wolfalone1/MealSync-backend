using FluentValidation;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliveryFailOrder;

public class ShopDeliveryFailOrderValidator : AbstractValidator<ShopDeliveryFailOrderCommand>
{
    public ShopDeliveryFailOrderValidator()
    {
    }
}