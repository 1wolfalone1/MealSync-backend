using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.Delete;

public class DeleteDeliveryStaffCommand : ICommand<Result>
{
    public long Id { get; set; }

    public bool IsConfirm { get; set; }
}