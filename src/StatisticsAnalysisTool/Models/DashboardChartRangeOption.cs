using StatisticsAnalysisTool.Localization;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardChartRangeOption
{
    public string Name
    {
        get;
        init;
    } = string.Empty;

    public int BucketCount
    {
        get;
        init;
    }

    public bool UseHourlyValues
    {
        get;
        init;
    }

    public static IReadOnlyList<DashboardChartRangeOption> CreateDefault()
    {
        return
        [
            new DashboardChartRangeOption()
            {
                Name = $"12 {LocalizationController.Translation("HOURS")}",
                BucketCount = 12,
                UseHourlyValues = true
            },
            new DashboardChartRangeOption()
            {
                Name = $"24 {LocalizationController.Translation("HOURS")}",
                BucketCount = 24,
                UseHourlyValues = true
            },
            new DashboardChartRangeOption()
            {
                Name = $"3 {LocalizationController.Translation("DAYS")}",
                BucketCount = 3,
                UseHourlyValues = false
            },
            new DashboardChartRangeOption()
            {
                Name = LocalizationController.Translation("LAST_7_DAYS"),
                BucketCount = 7,
                UseHourlyValues = false
            },
            new DashboardChartRangeOption()
            {
                Name = LocalizationController.Translation("LAST_30_DAYS"),
                BucketCount = 30,
                UseHourlyValues = false
            }
        ];
    }
}
