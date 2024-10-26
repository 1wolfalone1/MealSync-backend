using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Models;

public class ShopStaffResponse
{
    public long Id { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string AvatarUrl { get; set; }

    public ShopDeliveryStaffStatus Status { get; set; }

    public List<long> CurrentDeliveryPackageIds { get; set; }

    public List<long> FutureDeliveryPackageIds { get; set; }
}