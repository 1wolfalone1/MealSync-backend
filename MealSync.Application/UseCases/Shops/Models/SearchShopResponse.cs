using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Shops.Models;

public class SearchShopResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    public string PhoneNumber { get; set; }

    public bool IsAcceptingOrderNextDay { get; set; }

    public List<FoodShopResponse> Foods { get; set; }

    public class FoodShopResponse
    {
        public long Id { get; set; }

        public long ShopId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public double Price { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsSoldOut { get; set; }
    }
}