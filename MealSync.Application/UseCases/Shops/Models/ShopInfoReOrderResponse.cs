namespace MealSync.Application.UseCases.Shops.Models;

public class ShopInfoReOrderResponse
{
    public bool IsAllowReOrder { get; set; }

    public string? MessageNotAllow { get; set; }

    public List<ShopOperatingSlotReOrderResponse> OperatingSlots { get; set; }

    public List<ShopDormitoryReOrderResponse> Dormitories { get; set; }

    public class ShopDormitoryReOrderResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
    }

    public class ShopOperatingSlotReOrderResponse
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }

        public bool IsAcceptingOrderToday { get; set; }

        public bool IsAcceptingOrderTomorrow { get; set; }
    }
}