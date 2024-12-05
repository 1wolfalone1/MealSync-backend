using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Models;

public class FoodDetailOfShopResponse
{
    public long Id { get; set; }

    public long PlatformCategoryId { get; set; }

    public long ShopCategoryId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public double Price { get; set; }

    public string? ImageUrl { get; set; }

    public FoodStatus Status { get; set; }

    public bool IsSoldOut { get; set; }

    public FoodPackingUnitOfShopResponse FoodPackingUnit { get; set; }

    public List<OperatingSlotOfShopResponse> OperatingSlots { get; set; }

    public List<OptionGroupOfShopResponse> OptionGroups { get; set; }

    public class OperatingSlotOfShopResponse
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }
    }

    public class OptionGroupOfShopResponse
    {
        public long OptionGroupId { get; set; }

        public string Title { get; set; }

        public int DisplayOrder { get; set; }
    }

    public class FoodPackingUnitOfShopResponse
    {
        public long Id { get; set; }

        public long ShopId { get; set; }

        public string Name { get; set; }

        public double Weight { get; set; }

        public FoodPackingUnitType Type { get; set; }
    }
}