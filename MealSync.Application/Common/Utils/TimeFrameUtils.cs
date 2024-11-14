namespace MealSync.Application.Common.Utils;

public class TimeFrameUtils
{
    public static string GetTimeFrameString(int startTime, int endTime)
    {
        var startFrame = GetTimeHoursFormat(startTime);
        var endFame = GetTimeHoursFormat(endTime);
        return startFrame + " - " + endFame;
    }

    public static string GetTimeHoursFormat(int time)
    {
        var hour = time / 100;
        var minute = time % 100;
        return string.Format("{0}:{1}", hour.ToString().PadLeft(2, '0'), minute.ToString().PadRight(2, '0'));
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

    public static (DateTime StartTime, DateTime EndTime) GetStartTimeEndTimeToDateTime(DateTime intendedReceiveDate, int startTime, int endTime)
    {
        var startDateTime = new DateTime(
            intendedReceiveDate.Year,
            intendedReceiveDate.Month,
            intendedReceiveDate.Day,
            startTime / 100,
            startTime % 100,
            0);

        if (endTime == 2400)
        {
            intendedReceiveDate = intendedReceiveDate.AddDays(1);
            endTime = 0;
        }

        var endDateTime = new DateTime(
            intendedReceiveDate.Year,
            intendedReceiveDate.Month,
            intendedReceiveDate.Day,
            endTime / 100,
            endTime % 100,
            0);

        return (startDateTime, endDateTime);
    }

    public static (DateTime IntendedReceiveDate, int StartTime, int EndTime) OrderTimeFrameForBatchProcess(DateTimeOffset currentTime, int hoursBack)
    {
        // Round the current time to the nearest 30-minute frame
        int minutes = currentTime.Minute;
        int roundedMinutes = (minutes < 30) ? 0 : 30;
        DateTimeOffset roundedTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, roundedMinutes, 0, currentTime.Offset);

        // Subtract the specified number of hours and 30 minutes to adjust the frame
        DateTimeOffset startDateTime = roundedTime.AddHours(-hoursBack).AddMinutes(-30);

        // Calculate startTime in HHMM format
        int startTime = (startDateTime.Hour * 100) + startDateTime.Minute;

        // Calculate endTime and handle midnight transition (00:00 -> 2400)
        DateTimeOffset endDateTime = startDateTime.AddMinutes(30);
        int endTime = (endDateTime.Hour == 0 && endDateTime.Minute == 0) ? 2400 : (endDateTime.Hour * 100) + endDateTime.Minute;

        return (startDateTime.DateTime, startTime, endTime);
    }

    public static double ConvertToHours(int hhmm)
    {
        int hours = hhmm / 100;         // Extract the hours part
        int minutes = hhmm % 100;       // Extract the minutes part

        return hours + minutes / 60.0;  // Convert minutes to hours as a fraction
    }

    public static int ConvertToHHMM(double hours)
    {
        int hh = (int)hours;                   // Extract the integer part as hours
        int mm = (int)((hours - hh) * 60);     // Convert fractional part to minutes

        return hh * 100 + mm;                  // Combine hours and minutes in hhmm format
    }
}