using MealSync.Application.Common.Utils;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopOwners.Models;

public class ShopConfigurationResponse
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string LogoUrl { get; set; }

    public string BannerUrl { get; set; }

    public string PhoneNumber { get; set; }

    public bool IsAcceptingOrderNextDay { get; set; }

    public int MaxOrderHoursInAdvance { get; set; }

    public int MinOrderHoursInAdvance { get; set; }

    public int AverageOrderHandleInFrame { get; set; }

    public int AverageTotalOrderHandleInDay { get; set; }

    public LocationResponse Location { get; set; }

    public List<ShopDormitoryResponse> ShopDormitoryies { get; set; }

    public List<OperatingDayResponse> OperatingDays { get; set; } = new();
}

public class LocationResponse
{
    public int Id { get; set; }

    public string Address { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}

public class ShopDormitoryResponse
{
    public long Id { get; set; }

    public string Name { get; set; }
}

public class OperatingDayResponse
{
    public DayOfWeeks DayOfWeeks { get; set; }

    public bool IsClose { get; set; }

    public List<OperatingFrameResponse> OperatingFrames { get; set; }
}

public class OperatingFrameResponse
{
    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public bool IsActive { get; set; }

    public string FrameTime => TimeFrameUtils.GetTimeFrameString(StartTime, EndTime);
}