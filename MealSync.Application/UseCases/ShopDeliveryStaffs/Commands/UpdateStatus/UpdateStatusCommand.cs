using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateStatus;

public class UpdateStatusCommand : ICommand<Result>
{
    public long Id { get; set; }

    public bool IsConfirm { get; set; }

    public ShopDeliveryStaffStatus Status { get; set; }
}