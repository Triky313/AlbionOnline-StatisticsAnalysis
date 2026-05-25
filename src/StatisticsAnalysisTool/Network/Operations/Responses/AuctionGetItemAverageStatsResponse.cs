using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Crafting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public sealed class AuctionGetItemAverageStatsResponse
{
    public AuctionGetItemAverageStatsResponse(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters == null)
            {
                return;
            }

            if (parameters.TryGetValue(255, out var requestId))
            {
                RequestId = requestId.ObjectToInt();
            }

            var counts = parameters.TryGetValue(0, out var countValues)
                ? ConvertCounts(countValues)
                : [];
            var averagePrices = parameters.TryGetValue(1, out var priceValues)
                ? ConvertLongValues(priceValues)
                : [];
            var timestamps = parameters.TryGetValue(2, out var timestampValues)
                ? ConvertLongValues(timestampValues)
                : [];

            Points = BuildPoints(counts, averagePrices, timestamps);
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public int RequestId { get; }

    public IReadOnlyList<BlackMarketHistoryPoint> Points { get; } = [];

    private static IReadOnlyList<BlackMarketHistoryPoint> BuildPoints(
        IReadOnlyList<int> counts,
        IReadOnlyList<long> averagePrices,
        IReadOnlyList<long> timestamps)
    {
        var count = new[] { counts.Count, averagePrices.Count, timestamps.Count }.Min();
        if (count <= 0)
        {
            return [];
        }

        var result = new List<BlackMarketHistoryPoint>(count);
        for (var i = 0; i < count; i++)
        {
            var timestamp = ConvertTimestamp(timestamps[i]);
            if (timestamp == DateTime.MinValue)
            {
                continue;
            }

            result.Add(new BlackMarketHistoryPoint
            {
                Date = timestamp.Date,
                ItemCount = Math.Max(0, counts[i]),
                AveragePrice = ConvertSilverTotalToUnitAveragePrice(averagePrices[i], counts[i]),
                LastUpdatedUtc = DateTime.UtcNow
            });
        }

        return result;
    }

    private static long ConvertSilverTotalToUnitAveragePrice(long totalPriceInternal, int itemCount)
    {
        if (totalPriceInternal <= 0 || itemCount <= 0)
        {
            return 0;
        }

        var averagePrice = (decimal) totalPriceInternal / FixPoint.InternalFactor / itemCount;
        return Math.Max(0, (long) Math.Round(averagePrice, MidpointRounding.AwayFromZero));
    }

    private static IReadOnlyList<int> ConvertCounts(object value)
    {
        if (value is byte[] bytes)
        {
            return bytes.Select(x => (int) x).ToList();
        }

        return ConvertLongValues(value)
            .Select(x => x is > int.MaxValue ? int.MaxValue : (int) Math.Max(0, x))
            .ToList();
    }

    private static IReadOnlyList<long> ConvertLongValues(object value)
    {
        if (value is not IEnumerable enumerable)
        {
            var singleValue = value.ObjectToLong();
            return singleValue.HasValue ? [singleValue.Value] : [];
        }

        var result = new List<long>();
        foreach (var item in enumerable)
        {
            var longValue = item.ObjectToLong();
            if (longValue.HasValue)
            {
                result.Add(longValue.Value);
            }
        }

        return result;
    }

    private static DateTime ConvertTimestamp(long value)
    {
        try
        {
            if (value <= 0)
            {
                return DateTime.MinValue;
            }

            return new DateTime(value, DateTimeKind.Utc);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
}
