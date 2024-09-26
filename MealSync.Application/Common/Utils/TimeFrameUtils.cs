namespace MealSync.Application.Common.Utils;

public class TimeFrameUtils
{
    public static string GetTimeFrameString(int startTime, int endTime)
    {
        var startFrame = GetTimeHoursFormat(startTime);
        var endFame = GetTimeHoursFormat(endTime);
        return startFrame + "-" + endFame;
    }

    public static string GetTimeHoursFormat(int time)
    {
        var hour = time / 100;
        var minute = time % 100;
        return string.Format("{0}:{1}", hour.ToString().PadLeft(2,'0'), minute.ToString().PadRight(2, '0'));
    }
}