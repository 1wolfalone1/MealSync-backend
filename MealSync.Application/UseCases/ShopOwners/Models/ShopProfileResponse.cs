namespace MealSync.Application.UseCases.ShopOwners.Models;

public class ShopProfileResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string ShopOwnerName { get; set; }

    public string LogoUrl { get; set; }

    public string BannerUrl { get; set; }

    public string Description { get; set; }

    public string PhoneNumber { get; set; }

    public LocationResponse Location { get; set; }

    public List<ShopDormitoryResponse> ShopDormitories { get; set; }
}