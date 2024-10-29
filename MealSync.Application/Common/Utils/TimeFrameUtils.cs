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

    public static int GetCurrentHours()
    {
        var currentDate = GetCurrentDate();
        var hour = currentDate.Hour * 100;
        var minute = currentDate.Minute % 100;
        return hour + minute;
    }

    public static int GetCurrentHoursInUTC7()
    {
        var currentDate = GetCurrentDateInUTC7();
        var hour = currentDate.Hour * 100;
        return hour + currentDate.Minute;
    }

    public static int GetTimeHoursInRound()
    {
        DateTimeOffset utcTime = DateTimeOffset.UtcNow;
        DateTimeOffset timeInUtcPlus7 = utcTime;

        // Get the current hour and minute
        int hour = timeInUtcPlus7.Hour;
        int minute = timeInUtcPlus7.Minute;

        // Logic to round up to the next section (either xx:30 or next hour)
        if (minute > 0 && minute <= 30)
        {
            minute = 30;
        }
        else if (minute > 30)
        {
            hour += 1;
            minute = 0;
        }

        // Return the result as a number in hhmm format
        int roundedTime = hour * 100 + minute;
        return roundedTime;
    }

    public static DateTimeOffset GetCurrentDate()
    {
        DateTimeOffset utcTime = DateTimeOffset.UtcNow;
        return utcTime;
    }

    public static DateTimeOffset GetCurrentDateInUTC7()
    {
        DateTimeOffset utcTime = DateTimeOffset.UtcNow;
        return utcTime.ToOffset(TimeSpan.FromHours(7));
    }

    public static int ConvertEndTime(int endTime)
    {
        if (endTime == 2400)
        {
            return 0;
        }

        return endTime;
    }

    public static int ConvertMinutesToHour(int minutes)
    {
        int hours = minutes / 60;
        int remainingMinutes = minutes % 60;

        // Combine hours and remaining minutes in the format hhMM
        return (hours * 100) + remainingMinutes;
    }

}