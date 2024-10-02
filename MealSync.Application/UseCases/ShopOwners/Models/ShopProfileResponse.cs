namespace MealSync.Application.UseCases.ShopOwners.Models;

public class ShopProfileResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string LogoUrl { get; set; }

    public string BannerUrl { get; set; }

    public string Description { get; set; }

    public string PhoneNumber { get; set; }

    public bool IsAcceptingOrderNextDay { get; set; }

    public int MinOrderHoursInAdvance { get; set; }

    public int MaxOrderHoursInAdvance { get; set; }
}