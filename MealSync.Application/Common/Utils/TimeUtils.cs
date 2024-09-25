namespace MealSync.Application.Common.Utils;

public static class TimeUtils
{
    private const int TIME_FRAME_IN_MINUTES = 30;

    public static bool IsValidTime(int time)
    {
        int hours = time / 100; // Extract hours (first two digits)
        int minutes = time % 100; // Extract minutes (last two digits)

        // Check if the time is in a valid range (hours: 00-23, minutes: 00-59)
        return hours >= 0 && hours <= 23 && minutes >= 0 && minutes <= 59;
    }

    public static int ConvertToMinutes(int time)
    {
        int hours = time / 100; // Extract hours
        int minutes = time % 100; // Extract minutes

        // Convert hours to minutes and add minutes
        return hours * 60 + minutes;
    }

    public static int ConvertFromMinutes(int totalMinutes)
    {
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        return hours * 100 + minutes; // Convert back to HHmm format
    }

    public static bool IsValidEndTime(int startTime, int endTime)
    {
        // Convert both StartTime and EndTime to total minutes
        int startMinutes = ConvertToMinutes(startTime);
        int endMinutes = ConvertToMinutes(endTime);

        // Check if EndTime is greater than StartTime and the difference is a multiple of TIME_FRAME_IN_MINUTES minutes
        return endMinutes > startMinutes && (endMinutes - startMinutes) % TIME_FRAME_IN_MINUTES == 0;
    }

    public static List<(int SegmentStart, int SegmentEnd)> ConvertToTimeSegment(int startTime, int endTime)
    {
        List<(int SegmentStart, int SegmentEnd)> timeSegments = [];

        int currentMinutes = ConvertToMinutes(startTime);
        int endMinutes = ConvertToMinutes(endTime);

        while (currentMinutes < endMinutes)
        {
            int nextSegmentEndMinutes = Math.Min(currentMinutes + TIME_FRAME_IN_MINUTES, endMinutes);
            int segmentStart = ConvertFromMinutes(currentMinutes);
            int segmentEnd = ConvertFromMinutes(nextSegmentEndMinutes);
            timeSegments.Add((segmentStart, segmentEnd));

            currentMinutes = nextSegmentEndMinutes;
        }

        return timeSegments;
    }

    public static bool HasOverlappingTimeSegment(List<(int StartTime, int EndTime)> timeIntervals)
    {
        // Sort time segment by their start time
        var sortedIntervals = timeIntervals.OrderBy(interval => interval.StartTime).ToList();

        // Iterate through the sorted time segment and check for overlap
        for (int i = 1; i < sortedIntervals.Count; i++)
        {
            var prev = sortedIntervals[i - 1];
            var current = sortedIntervals[i];

            // Check if the previous time segment overlaps with the current one
            if (prev.EndTime > current.StartTime)
            {
                return true; // Overlap found
            }
        }

        // No overlap found
        return false;
    }

}