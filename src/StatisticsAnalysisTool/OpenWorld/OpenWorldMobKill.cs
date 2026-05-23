using System;

namespace StatisticsAnalysisTool.OpenWorld;

public class OpenWorldMobKill
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long TimestampUtc { get; set; }
    public string MobUniqueName { get; set; } = string.Empty;
    public string MobName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;

    public DateTime TimestampDateTimeUtc => new(TimestampUtc, DateTimeKind.Utc);
}