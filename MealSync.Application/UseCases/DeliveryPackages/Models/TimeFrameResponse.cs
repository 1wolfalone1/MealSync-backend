using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.DeliveryPackages.Models;

public class TimeFrameResponse
{
    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public string TimeFrameFormat => TimeFrameUtils.GetTimeFrameString(StartTime, EndTime);
}