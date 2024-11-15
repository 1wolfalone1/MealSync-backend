using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateInfoForStaff;

public class UpdateInfoForStaffCommand : ICommand<Result>
{
    public string PhoneNumber { get; set; }

    public string FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public Genders Gender { get; set; }
}