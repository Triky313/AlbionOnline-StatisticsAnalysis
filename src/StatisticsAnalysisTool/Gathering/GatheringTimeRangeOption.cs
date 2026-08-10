using StatisticsAnalysisTool.Localization;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Gathering;

public sealed class GatheringTimeRangeOption
{
    public string Name { get; init; } = string.Empty;
    public int BucketCount { get; init; }
    public GatheringTimeRangeUnit Unit { get; init; }

    public static IReadOnlyList<GatheringTimeRangeOption> CreateDefault()
    {
        return
        [
            Create(10, GatheringTimeRangeUnit.Minute, $"10 {LocalizationController.Translation("MINUTES")}"),
            Create(30, GatheringTimeRangeUnit.Minute, $"30 {LocalizationController.Translation("MINUTES")}"),
            Create(1, GatheringTimeRangeUnit.Hour, $"1 {LocalizationController.Translation("HOUR")}"),
            Create(3, GatheringTimeRangeUnit.Hour, $"3 {LocalizationController.Translation("HOURS")}"),
            Create(12, GatheringTimeRangeUnit.Hour, $"12 {LocalizationController.Translation("HOURS")}"),
            Create(24, GatheringTimeRangeUnit.Hour, $"24 {LocalizationController.Translation("HOURS")}"),
            Create(3, GatheringTimeRangeUnit.Day, $"3 {LocalizationController.Translation("DAYS")}"),
            Create(7, GatheringTimeRangeUnit.Day, $"7 {LocalizationController.Translation("DAYS")}"),
            Create(30, GatheringTimeRangeUnit.Day, $"30 {LocalizationController.Translation("DAYS")}"),
            Create(365, GatheringTimeRangeUnit.Day, $"365 {LocalizationController.Translation("DAYS")}")
        ];
    }

    private static GatheringTimeRangeOption Create(int bucketCount, GatheringTimeRangeUnit unit, string name)
    {
        return new GatheringTimeRangeOption
        {
            Name = name,
            BucketCount = bucketCount,
            Unit = unit
        };
    }
}