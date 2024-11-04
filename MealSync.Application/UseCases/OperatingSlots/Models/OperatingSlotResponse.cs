using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.OperatingSlots.Models;

public class OperatingSlotResponse
{
    public long Id { get; set; }

    public string Title { get; set; }

    public int StartTime { get; set; }

    private int _endTime; // Backing field

    public int EndTime
    {
        get
        {
            return TimeFrameUtils.ConvertEndTime(_endTime); // Use the backing field
        }

        set
        {
            _endTime = value; // Set the backing field
        }
    }

    public bool IsActive { get; set; }

    public bool IsReceivingOrderPaused { get; set; }

    public string FrameFormat
    {
        get
        {
            return TimeFrameUtils.GetTimeFrameString(StartTime, EndTime);
        }
    }
}