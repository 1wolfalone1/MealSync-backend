using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.OperatingSlots.Models;

public class OperatingSlotResponse
{
    public long Id { get; set; }

    public string Title { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public string FrameFormat
    {
        get
        {
            return TimeFrameUtils.GetTimeFrameString(StartTime, EndTime);
        }
    }
}