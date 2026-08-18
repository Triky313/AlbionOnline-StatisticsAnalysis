using System;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class ActionInterval
{
    public ActionInterval(DateTime startTime)
    {
        StartTime = startTime;
    }

    public DateTime StartTime { get; }
    public DateTime? EndTime { get; set; }
    public TimeSpan TimeSpan => GetDuration(DateTime.UtcNow);

    public TimeSpan GetDuration(DateTime currentTime)
    {
        var effectiveEndTime = EndTime ?? currentTime;
        return effectiveEndTime > StartTime
            ? effectiveEndTime - StartTime
            : TimeSpan.Zero;
    }
}