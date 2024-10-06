using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Favourites.Models;

public class ShopFavouriteResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    public string? Description { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public double AverageRating { get; set; }

    public ShopStatus Status { get; set; }

    public bool IsReceivingOrderPaused { get; set; }
}