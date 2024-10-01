using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.ShopOwners.Models;

public class ShopConfigurationResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string LogoUrl { get; set; }

    public string BannerUrl { get; set; }

    public string PhoneNumber { get; set; }

    public bool IsAcceptingOrderNextDay { get; set; }

    public bool IsReceivingOrderPaused { get; set; }

    public bool IsAutoOrderConfirmation { get; set; }

    public int MaxOrderHoursInAdvance { get; set; }

    public int MinOrderHoursInAdvance { get; set; }

    public LocationResponse Location { get; set; }

    public List<ShopDormitoryResponse> ShopDormitoryies { get; set; }

    public List<ShopOperatingSlotResponse> OperatingSlots { get; set; } = new();
}

public class LocationResponse
{
    public long Id { get; set; }

    public string Address { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}

public class ShopDormitoryResponse
{
    public long Id { get; set; }

    public string Name { get; set; }
}

public class ShopOperatingSlotResponse
{
    public long Id { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public string TimeSlot
    {
        get => TimeFrameUtils.GetTimeFrameString(StartTime, EndTime);
    }
}