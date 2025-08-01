using System;

namespace StatisticsAnalysisTool.Common;

public static class RateLimitedAction
{
    private static DateTime _nextAllowedRunUtc = DateTime.MinValue;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    public static void Run(Action task, TimeSpan? intervalOverride = null)
    {
        var interval = intervalOverride ?? Interval;
        var now = DateTime.UtcNow;

        lock (typeof(RateLimitedAction))
        {
            if (now < _nextAllowedRunUtc)
            {
                return;
            }

            _nextAllowedRunUtc = now.Add(interval);
        }

        task?.Invoke();
    }
}