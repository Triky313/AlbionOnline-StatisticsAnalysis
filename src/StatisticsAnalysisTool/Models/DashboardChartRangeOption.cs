using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardChartRangeOption
{
    public string Name { get; init; } = string.Empty;
    public int BucketCount { get; init; }
    public DashboardChartRangeUnit Unit { get; init; }

    public static IReadOnlyList<DashboardChartRangeOption> CreateDefault()
    {
        return
        [
            new DashboardChartRangeOption()
            {
                Name = $"10 {LocalizationController.Translation("MINUTES")}",
                BucketCount = 10,
                Unit = DashboardChartRangeUnit.Minute
            },
            new DashboardChartRangeOption()
            {
                Name = $"30 {LocalizationController.Translation("MINUTES")}",
                BucketCount = 30,
                Unit = DashboardChartRangeUnit.Minute
            },
            new DashboardChartRangeOption()
            {
                Name = $"1 {LocalizationController.Translation("HOUR")}",
                BucketCount = 1,
                Unit = DashboardChartRangeUnit.Hour
            },
            new DashboardChartRangeOption()
            {
                Name = $"3 {LocalizationController.Translation("HOURS")}",
                BucketCount = 3,
                Unit = DashboardChartRangeUnit.Hour
            },
            new DashboardChartRangeOption()
            {
                Name = $"12 {LocalizationController.Translation("HOURS")}",
                BucketCount = 12,
                Unit = DashboardChartRangeUnit.Hour
            },
            new DashboardChartRangeOption()
            {
                Name = $"24 {LocalizationController.Translation("HOURS")}",
                BucketCount = 24,
                Unit = DashboardChartRangeUnit.Hour
            },
            new DashboardChartRangeOption()
            {
                Name = $"3 {LocalizationController.Translation("DAYS")}",
                BucketCount = 3,
                Unit = DashboardChartRangeUnit.Day
            },
            new DashboardChartRangeOption()
            {
                Name = LocalizationController.Translation("LAST_7_DAYS"),
                BucketCount = 7,
                Unit = DashboardChartRangeUnit.Day
            },
            new DashboardChartRangeOption()
            {
                Name = LocalizationController.Translation("LAST_30_DAYS"),
                BucketCount = 30,
                Unit = DashboardChartRangeUnit.Day
            },
            new DashboardChartRangeOption()
            {
                Name = LocalizationController.Translation("LAST_365_DAYS"),
                BucketCount = 365,
                Unit = DashboardChartRangeUnit.Day
            }
        ];
    }
}