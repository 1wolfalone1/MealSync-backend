using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Shops.Models;

public class ShopInfoResponse
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

    public List<ShopOperatingSlotResponse> OperatingSlots { get; set; }

    public ShopLocationResponse Location { get; set; }

    public List<ShopDormitoryResponse> Dormitories { get; set; }

    public class ShopDormitoryResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
    }

    public class ShopLocationResponse
    {
        public long Id { get; set; }

        public string Address { get; set; } = null!;

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class ShopOperatingSlotResponse
    {
        public long Id { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }
    }
}