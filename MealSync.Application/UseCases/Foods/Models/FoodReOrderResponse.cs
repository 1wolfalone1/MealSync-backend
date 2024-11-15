namespace MealSync.Application.UseCases.Foods.Models;

public class FoodReOrderResponse
{
    public bool IsAllowReOrder { get; set; }

    public string? MessageNotAllow { get; set; }

    public ShopInfoReOrderResponse ShopInfo { get; set; }

    public string? Note { get; set; }

    public List<FoodDetailReorderResponse>? Foods { get; set; }

    public class ShopInfoReOrderResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string? LogoUrl { get; set; }

        public string? BannerUrl { get; set; }

        public string? Description { get; set; }

        public LocationResponse Location { get; set; }

        public List<OperatingSlotReOrderResponse> OperatingSlots { get; set; }

        public List<DormitoryReOrderResponse> Dormitories { get; set; }
    }

    public class LocationResponse
    {
        public long Id { get; set; }

        public string Address { get; set; } = null!;

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class DormitoryReOrderResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
    }

    public class OperatingSlotReOrderResponse
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }

        public bool IsAcceptingOrderToday { get; set; }

        public bool IsAcceptingOrderTomorrow { get; set; }
    }

    public class FoodDetailReorderResponse
    {
        public long Id { get; set; }

        public int Quantity { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public double Price { get; set; }

        public string? ImageUrl { get; set; }

        public string? Note { get; set; }

        public List<OptionGroupRadioReOrderResponse> OptionGroupRadio { get; set; }

        public List<OptionGroupCheckboxReOrderResponse> OptionGroupCheckbox { get; set; }
    }

    public class OptionGroupRadioReOrderResponse
    {
        public long Id { get; set; }

        public string? Title { get; set; }

        public OptionReOrderResponse Option { get; set; }
    }

    public class OptionGroupCheckboxReOrderResponse
    {
        public long Id { get; set; }

        public string? Title { get; set; }

        public List<OptionReOrderResponse> Options { get; set; }
    }

    public class OptionReOrderResponse
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsCalculatePrice { get; set; }

        public double Price { get; set; }
    }
}