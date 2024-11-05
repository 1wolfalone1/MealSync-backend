using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliverySuccess;

public class ShopAndStaffDeliverySuccessCommand : ICommand<Result>
{
    public long OrderId { get; set; }

    public long CustomerId { get; set; }

    public long ShipperId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Token { get; set; }

    public bool IsConfirm { get; set; }
}