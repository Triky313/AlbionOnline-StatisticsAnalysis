using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using System.Collections.Generic;
using System.Globalization;

namespace StatisticsAnalysisTool.Models;

public static class DashboardLootStatisticsCalculator
{
    public static double GetRangeHours(DashboardChartRangeOption selectedRange)
    {
        if (selectedRange == null || selectedRange.BucketCount <= 0)
        {
            return 0;
        }

        return selectedRange.Unit switch
        {
            DashboardChartRangeUnit.Minute => selectedRange.BucketCount / 60d,
            DashboardChartRangeUnit.Hour => selectedRange.BucketCount,
            DashboardChartRangeUnit.Day => selectedRange.BucketCount * 24d,
            _ => 0
        };
    }

    public static int GetValueClassIndex(double unitValue)
    {
        if (unitValue < 10_000)
        {
            return 0;
        }

        if (unitValue < 100_000)
        {
            return 1;
        }

        return unitValue <= 500_000 ? 2 : 3;
    }

    public static IReadOnlyCollection<DashboardLootBreakdownItem> CreateValueDistribution(
        IReadOnlyList<long> itemCounts,
        IReadOnlyList<double> totalValues,
        double totalLootValue)
    {
        var culture = CultureInfo.CurrentCulture;
        var names = new[]
        {
            string.Format(culture, LocalizationController.Translation("LOOT_VALUE_UNDER_10000"), 10_000),
            string.Format(culture, LocalizationController.Translation("LOOT_VALUE_10000_TO_100000"), 10_000, 100_000),
            string.Format(culture, LocalizationController.Translation("LOOT_VALUE_100000_TO_500000"), 100_000, 500_000),
            string.Format(culture, LocalizationController.Translation("LOOT_VALUE_OVER_500000"), 500_000)
        };
        var result = new DashboardLootBreakdownItem[names.Length];

        for (var index = 0; index < names.Length; index++)
        {
            result[index] = new DashboardLootBreakdownItem(
                names[index],
                itemCounts[index],
                CalculateSharePercentage(totalValues[index], totalLootValue));
        }

        return result;
    }

    public static IReadOnlyCollection<DashboardLootBreakdownItem> CreateCountDistribution(
        IReadOnlyList<string> names,
        IReadOnlyList<long> itemCounts)
    {
        var totalItemCount = 0L;
        for (var index = 0; index < itemCounts.Count; index++)
        {
            totalItemCount += itemCounts[index];
        }

        var result = new DashboardLootBreakdownItem[names.Count];
        for (var index = 0; index < names.Count; index++)
        {
            result[index] = new DashboardLootBreakdownItem(
                names[index],
                itemCounts[index],
                CalculateSharePercentage(itemCounts[index], totalItemCount));
        }

        return result;
    }

    public static double CalculateSharePercentage(double value, double total)
    {
        return total > 0 ? value * 100d / total : 0;
    }
}
