using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeProfitTimeSeriesService
{
    private readonly TradeAnalyticsValueService _tradeAnalyticsValueService = new();

    private enum TradeProfitTimeBucketUnit
    {
        Minute,
        Hour,
        Day,
        Month
    }

    private static readonly TradeProfitTimeAggregation[] AutoAggregationOrder =
    [
        TradeProfitTimeAggregation.Hour,
        TradeProfitTimeAggregation.Day,
        TradeProfitTimeAggregation.Week,
        TradeProfitTimeAggregation.Month
    ];

    public const int MaxVisiblePoints = 365;

    public TradeProfitTimeSeriesResult BuildTimeSeries(IEnumerable<Trade> trades, DateTime rangeStart, DateTime rangeEndInclusive, TradeProfitTimeAggregation requestedAggregation)
    {
        var normalizedRange = NormalizeRange(rangeStart, rangeEndInclusive);
        var configuration = ResolveConfiguration(requestedAggregation, normalizedRange);
        var bucketPeriods = configuration.UseFixedWindow
            ? CreateFixedWindowBucketPeriods(normalizedRange, configuration)
            : CreateRangeBucketPeriods(normalizedRange, configuration);
        var buckets = bucketPeriods
            .Select(period => new TradeProfitBucketAccumulator(period.PeriodStart, period.PeriodEnd))
            .ToList();

        var filteredTrades = (trades ?? [])
            .Where(trade => trade != null)
            .Where(trade => trade.Timestamp >= normalizedRange.RangeStartInclusive && trade.Timestamp < normalizedRange.RangeEndExclusive)
            .OrderBy(trade => trade.Timestamp)
            .ToList();

        foreach (var trade in filteredTrades)
        {
            if (!TryGetBucketIndex(bucketPeriods, trade.Timestamp, out var bucketIndex))
            {
                continue;
            }

            var tradeValues = _tradeAnalyticsValueService.GetBreakdown(trade);
            buckets[bucketIndex].Sold += tradeValues.Sold;
            buckets[bucketIndex].Bought += tradeValues.Bought;
            buckets[bucketIndex].Tax += tradeValues.Tax;
            buckets[bucketIndex].TradeCount++;
        }

        var points = new List<TradeProfitTimeSeriesPoint>(bucketPeriods.Count);
        var cumulativeNetProfit = 0d;

        for (var index = 0; index < buckets.Count; index++)
        {
            var bucket = buckets[index];
            cumulativeNetProfit += bucket.NetProfit;

            points.Add(new TradeProfitTimeSeriesPoint
            {
                PeriodStart = bucket.PeriodStart,
                PeriodEnd = bucket.PeriodEnd.AddTicks(-1),
                Sold = bucket.Sold,
                Bought = bucket.Bought,
                Tax = bucket.Tax,
                NetProfit = bucket.NetProfit,
                CumulativeNetProfit = cumulativeNetProfit,
                AverageProfitPerTrade = bucket.TradeCount > 0 ? bucket.NetProfit / bucket.TradeCount : 0d,
                TradeCount = bucket.TradeCount
            });
        }

        return new TradeProfitTimeSeriesResult
        {
            Points = points,
            RequestedAggregation = requestedAggregation,
            EffectiveAggregation = configuration.EffectiveAggregation,
            BucketStepSize = configuration.BucketStepSize
        };
    }

    private static TradeProfitTimeSeriesConfiguration ResolveConfiguration(TradeProfitTimeAggregation requestedAggregation, TradeProfitTimeSeriesRange range)
    {
        return requestedAggregation == TradeProfitTimeAggregation.Auto
            ? ResolveAutoConfiguration(range)
            : ResolveFixedWindowConfiguration(requestedAggregation);
    }

    private static TradeProfitTimeSeriesConfiguration ResolveFixedWindowConfiguration(TradeProfitTimeAggregation requestedAggregation)
    {
        return requestedAggregation switch
        {
            TradeProfitTimeAggregation.Hour => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Hour, TradeProfitTimeBucketUnit.Minute, 60, 1, true),
            TradeProfitTimeAggregation.Day => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Day, TradeProfitTimeBucketUnit.Hour, 24, 1, true),
            TradeProfitTimeAggregation.Week => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Week, TradeProfitTimeBucketUnit.Day, 7, 1, true),
            TradeProfitTimeAggregation.Month => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Month, TradeProfitTimeBucketUnit.Day, 31, 1, true),
            TradeProfitTimeAggregation.Year => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Year, TradeProfitTimeBucketUnit.Month, 12, 1, true),
            _ => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Day, TradeProfitTimeBucketUnit.Hour, 24, 1, true)
        };
    }

    private static TradeProfitTimeSeriesConfiguration ResolveAutoConfiguration(TradeProfitTimeSeriesRange range)
    {
        foreach (var aggregation in AutoAggregationOrder)
        {
            var candidate = ResolveAutoCandidateConfiguration(aggregation);
            var bucketCount = CountBuckets(range, candidate.BucketUnit, candidate.BucketStepSize);
            if (bucketCount <= MaxVisiblePoints)
            {
                return candidate;
            }
        }

        var fallbackConfiguration = ResolveAutoCandidateConfiguration(TradeProfitTimeAggregation.Month);
        var fallbackBucketCount = CountBuckets(range, fallbackConfiguration.BucketUnit, fallbackConfiguration.BucketStepSize);
        var fallbackStepSize = Math.Max(1, (int)Math.Ceiling(fallbackBucketCount / (double)MaxVisiblePoints));

        return fallbackConfiguration with
        {
            BucketStepSize = fallbackStepSize
        };
    }

    private static TradeProfitTimeSeriesConfiguration ResolveAutoCandidateConfiguration(TradeProfitTimeAggregation aggregation)
    {
        return aggregation switch
        {
            TradeProfitTimeAggregation.Hour => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Hour, TradeProfitTimeBucketUnit.Hour, 0, 1, false),
            TradeProfitTimeAggregation.Day => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Day, TradeProfitTimeBucketUnit.Day, 0, 1, false),
            TradeProfitTimeAggregation.Week => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Week, TradeProfitTimeBucketUnit.Day, 0, 7, false),
            TradeProfitTimeAggregation.Month => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Month, TradeProfitTimeBucketUnit.Month, 0, 1, false),
            _ => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Day, TradeProfitTimeBucketUnit.Day, 0, 1, false)
        };
    }

    private static TradeProfitTimeSeriesRange NormalizeRange(DateTime rangeStart, DateTime rangeEndInclusive)
    {
        var orderedStart = rangeStart <= rangeEndInclusive ? rangeStart : rangeEndInclusive;
        var orderedEnd = rangeEndInclusive >= rangeStart ? rangeEndInclusive : rangeStart;
        var normalizedStart = orderedStart.Date;
        var normalizedEndInclusive = orderedEnd.Date;
        var normalizedEndExclusive = normalizedEndInclusive == DateTime.MaxValue.Date
            ? DateTime.MaxValue
            : normalizedEndInclusive.AddDays(1);

        if (normalizedEndExclusive <= normalizedStart)
        {
            normalizedEndExclusive = normalizedStart.AddDays(1);
        }

        return new TradeProfitTimeSeriesRange(normalizedStart, normalizedEndExclusive);
    }

    private static int CountBuckets(TradeProfitTimeSeriesRange range, TradeProfitTimeBucketUnit bucketUnit, int bucketStepSize)
    {
        var bucketCount = 0;
        var currentStart = AlignToBucketStart(range.RangeStartInclusive, bucketUnit);

        while (currentStart < range.RangeEndExclusive)
        {
            currentStart = AddBucket(currentStart, bucketUnit, bucketStepSize);
            bucketCount++;
        }

        return bucketCount;
    }

    private static List<TradeProfitBucketPeriod> CreateFixedWindowBucketPeriods(TradeProfitTimeSeriesRange range, TradeProfitTimeSeriesConfiguration configuration)
    {
        var latestTimestamp = range.RangeEndExclusive == DateTime.MaxValue
            ? DateTime.MaxValue.AddTicks(-1)
            : range.RangeEndExclusive.AddTicks(-1);
        var latestBucketStart = AlignToBucketStart(latestTimestamp, configuration.BucketUnit);
        var firstBucketStart = AddBucket(latestBucketStart, configuration.BucketUnit, -((configuration.BucketCount - 1) * configuration.BucketStepSize));
        var periods = new List<TradeProfitBucketPeriod>(configuration.BucketCount);

        for (var index = 0; index < configuration.BucketCount; index++)
        {
            var periodStart = AddBucket(firstBucketStart, configuration.BucketUnit, index * configuration.BucketStepSize);
            var periodEnd = AddBucket(periodStart, configuration.BucketUnit, configuration.BucketStepSize);
            periods.Add(new TradeProfitBucketPeriod(periodStart, periodEnd));
        }

        return periods;
    }

    private static List<TradeProfitBucketPeriod> CreateRangeBucketPeriods(TradeProfitTimeSeriesRange range, TradeProfitTimeSeriesConfiguration configuration)
    {
        var periods = new List<TradeProfitBucketPeriod>();
        var currentStart = AlignToBucketStart(range.RangeStartInclusive, configuration.BucketUnit);

        while (currentStart < range.RangeEndExclusive)
        {
            var nextStart = AddBucket(currentStart, configuration.BucketUnit, configuration.BucketStepSize);
            if (nextStart > range.RangeEndExclusive)
            {
                nextStart = range.RangeEndExclusive;
            }

            periods.Add(new TradeProfitBucketPeriod(currentStart, nextStart));
            currentStart = nextStart;
        }

        return periods;
    }

    private static bool TryGetBucketIndex(IReadOnlyList<TradeProfitBucketPeriod> bucketPeriods, DateTime timestamp, out int bucketIndex)
    {
        var lowerBound = 0;
        var upperBound = bucketPeriods.Count - 1;

        while (lowerBound <= upperBound)
        {
            var middleIndex = lowerBound + ((upperBound - lowerBound) / 2);
            var bucket = bucketPeriods[middleIndex];

            if (timestamp < bucket.PeriodStart)
            {
                upperBound = middleIndex - 1;
                continue;
            }

            if (timestamp >= bucket.PeriodEnd)
            {
                lowerBound = middleIndex + 1;
                continue;
            }

            bucketIndex = middleIndex;
            return true;
        }

        bucketIndex = -1;
        return false;
    }

    private static DateTime AlignToBucketStart(DateTime timestamp, TradeProfitTimeBucketUnit bucketUnit)
    {
        return bucketUnit switch
        {
            TradeProfitTimeBucketUnit.Minute => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, 0),
            TradeProfitTimeBucketUnit.Hour => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0),
            TradeProfitTimeBucketUnit.Day => timestamp.Date,
            TradeProfitTimeBucketUnit.Month => new DateTime(timestamp.Year, timestamp.Month, 1),
            _ => timestamp
        };
    }

    private static DateTime AddBucket(DateTime bucketStart, TradeProfitTimeBucketUnit bucketUnit, int step)
    {
        return bucketUnit switch
        {
            TradeProfitTimeBucketUnit.Minute => bucketStart.AddMinutes(step),
            TradeProfitTimeBucketUnit.Hour => bucketStart.AddHours(step),
            TradeProfitTimeBucketUnit.Day => bucketStart.AddDays(step),
            TradeProfitTimeBucketUnit.Month => bucketStart.AddMonths(step),
            _ => bucketStart
        };
    }

    private sealed class TradeProfitBucketAccumulator(DateTime periodStart, DateTime periodEnd)
    {
        public DateTime PeriodStart { get; } = periodStart;
        public DateTime PeriodEnd { get; } = periodEnd;
        public double Sold { get; set; }
        public double Bought { get; set; }
        public double Tax { get; set; }
        public int TradeCount { get; set; }

        public double NetProfit => Sold - Bought - Tax;
    }

    private readonly record struct TradeProfitTimeSeriesConfiguration(
        TradeProfitTimeAggregation EffectiveAggregation,
        TradeProfitTimeBucketUnit BucketUnit,
        int BucketCount,
        int BucketStepSize,
        bool UseFixedWindow);

    private readonly record struct TradeProfitTimeSeriesRange(DateTime RangeStartInclusive, DateTime RangeEndExclusive);
    private readonly record struct TradeProfitBucketPeriod(DateTime PeriodStart, DateTime PeriodEnd);
}