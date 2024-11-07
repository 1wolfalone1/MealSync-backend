using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Models;

public class ShopDeliveryStaffInfoResponse
{
    public long Id { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? FullName { get; set; }

    public Genders Genders { get; set; }

    public AccountStatus AccountStatus { get; set; }

    public ShopDeliveryStaffStatus ShopDeliveryStaffStatus { get; set; }
}