namespace MealSync.Application.UseCases.Shops.Models;

public class ShopSummaryResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    public string? Description { get; set; }

    public string PhoneNumber { get; set; }

    public double AverageRating { get; set; }

    public int TotalOrder { get; set; }
}