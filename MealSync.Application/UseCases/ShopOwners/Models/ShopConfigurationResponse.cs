using MealSync.Application.Common.Utils;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopOwners.Models;

public class ShopConfigurationResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string ShopOwnerName { get; set; }

    public string LogoUrl { get; set; }

    public string BannerUrl { get; set; }

    public string PhoneNumber { get; set; }

    public ShopStatus Status { get; set; }

    public string Description { get; set; }

    public bool IsAcceptingOrderNextDay { get; set; }

    public bool IsReceivingOrderPaused { get; set; }

    public double MinValueOrderFreeShip { get; set; }

    public double AdditionalShipFee { get; set; }

    public bool IsAutoOrderConfirmation { get; set; }

    public int MaxOrderHoursInAdvance { get; set; }

    public int MinOrderHoursInAdvance { get; set; }

    public LocationResponse Location { get; set; }

    public List<ShopDormitoryResponse> ShopDormitories { get; set; }

    private List<ShopOperatingSlotResponse> _operatingSlots;

    public List<ShopOperatingSlotResponse> OperatingSlots
    {
        set => _operatingSlots = value;
        get
        {
            if (_operatingSlots != default)
                return _operatingSlots.OrderBy(os => os.StartTime).ToList();
            return new List<ShopOperatingSlotResponse>();
        }
    }
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
    public long DormitoryId { get; set; }

    public string Name { get; set; }
}

public class ShopOperatingSlotResponse
{
    public long Id { get; set; }

    public string Title { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public bool IsActive { get; set; }

    public bool IsReceivingOrderPaused { get; set; }

    public string TimeSlot
    {
        get => TimeFrameUtils.GetTimeFrameString(StartTime, EndTime);
    }
}