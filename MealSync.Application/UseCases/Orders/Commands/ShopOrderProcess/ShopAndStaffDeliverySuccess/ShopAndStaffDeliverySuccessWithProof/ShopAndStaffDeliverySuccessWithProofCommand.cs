using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliverySuccess.ShopAndStaffDeliverySuccessWithProof;

public class ShopAndStaffDeliverySuccessWithProofCommand : ICommand<Result>
{
    public long OrderId { get; set; }

    public string[] ImageProofs { get; set; }
}