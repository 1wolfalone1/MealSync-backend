using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateInfo;

public class UpdateDeliveryStaffCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public Genders Gender { get; set; }

    public ShopDeliveryStaffStatus Status { get; set; }
}