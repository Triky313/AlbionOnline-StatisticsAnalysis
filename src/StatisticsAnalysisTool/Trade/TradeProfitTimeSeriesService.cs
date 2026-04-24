using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeProfitTimeSeriesService
{
    private enum TradeProfitTimeBucketUnit
    {
        Minute,
        Hour,
        Day,
        Month
    }

    private readonly record struct TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation EffectiveAggregation, TradeProfitTimeBucketUnit BucketUnit, int BucketCount);

    public const int MaxVisiblePoints = 365;

    public TradeProfitTimeSeriesResult BuildTimeSeries(IEnumerable<Trade> trades, DateTime rangeStart, DateTime rangeEndInclusive, TradeProfitTimeAggregation requestedAggregation)
    {
        var configuration = ResolveConfiguration(requestedAggregation);
        var filteredTrades = (trades ?? [])
            .Where(x => x != null)
            .OrderBy(x => x.Timestamp)
            .ToList();

        var latestTimestamp = filteredTrades.Count > 0 ? filteredTrades[^1].Timestamp : ResolveFallbackTimestamp(rangeStart, rangeEndInclusive);

        var latestBucketStart = AlignToBucketStart(latestTimestamp, configuration.BucketUnit);
        var bucketPeriods = CreateBucketPeriods(latestBucketStart, configuration);
        var buckets = bucketPeriods.ToDictionary(
            x => x.PeriodStart,
            x => new TradeProfitBucketAccumulator(x.PeriodStart, x.PeriodEnd));

        foreach (var trade in filteredTrades)
        {
            var bucketKey = AlignToBucketStart(trade.Timestamp, configuration.BucketUnit);
            if (!buckets.TryGetValue(bucketKey, out var bucket))
            {
                continue;
            }

            var tradeValues = GetTradeValues(trade);
            bucket.Sold += tradeValues.Sold;
            bucket.Bought += tradeValues.Bought;
            bucket.Tax += tradeValues.Tax;
            bucket.TradeCount++;
        }

        var points = new List<TradeProfitTimeSeriesPoint>(bucketPeriods.Count);
        var cumulativeNetProfit = 0d;

        foreach (var bucketPeriod in bucketPeriods)
        {
            var bucket = buckets[bucketPeriod.PeriodStart];
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
            BucketStepSize = 1
        };
    }

    private static TradeProfitTimeSeriesConfiguration ResolveConfiguration(TradeProfitTimeAggregation requestedAggregation)
    {
        return requestedAggregation switch
        {
            TradeProfitTimeAggregation.Hour => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Hour, TradeProfitTimeBucketUnit.Minute, 60),
            TradeProfitTimeAggregation.Day => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Day, TradeProfitTimeBucketUnit.Hour, 24),
            TradeProfitTimeAggregation.Week => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Week, TradeProfitTimeBucketUnit.Day, 7),
            TradeProfitTimeAggregation.Month => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Month, TradeProfitTimeBucketUnit.Month, 4),
            TradeProfitTimeAggregation.Year => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Year, TradeProfitTimeBucketUnit.Month, 12),
            _ => new TradeProfitTimeSeriesConfiguration(TradeProfitTimeAggregation.Day, TradeProfitTimeBucketUnit.Hour, 24)
        };
    }

    private static DateTime ResolveFallbackTimestamp(DateTime rangeStart, DateTime rangeEndInclusive)
    {
        if (rangeEndInclusive >= rangeStart)
        {
            return rangeEndInclusive;
        }

        return rangeStart;
    }

    private static List<TradeProfitBucketPeriod> CreateBucketPeriods(DateTime latestBucketStart, TradeProfitTimeSeriesConfiguration configuration)
    {
        var periods = new List<TradeProfitBucketPeriod>(configuration.BucketCount);
        var firstBucketStart = AddBucket(latestBucketStart, configuration.BucketUnit, -(configuration.BucketCount - 1));

        for (var index = 0; index < configuration.BucketCount; index++)
        {
            var periodStart = AddBucket(firstBucketStart, configuration.BucketUnit, index);
            var periodEnd = AddBucket(periodStart, configuration.BucketUnit, 1);

            periods.Add(new TradeProfitBucketPeriod(periodStart, periodEnd));
        }

        return periods;
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

    private static TradeProfitValues GetTradeValues(Trade trade)
    {
        if (trade == null)
        {
            return TradeProfitValues.Empty;
        }

        return trade.Type switch
        {
            TradeType.Mail when trade.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired => new TradeProfitValues(
                trade.MailContent.TotalPrice.IntegerValue,
                0d,
                trade.MailContent.TaxSetupPrice.IntegerValue + trade.MailContent.TaxPrice.IntegerValue),
            TradeType.Mail when trade.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired => new TradeProfitValues(
                0d,
                trade.MailContent.TotalPrice.IntegerValue,
                trade.MailContent.TaxSetupPrice.IntegerValue + trade.MailContent.TaxPrice.IntegerValue),
            TradeType.InstantSell => new TradeProfitValues(
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d,
                trade.InstantBuySellContent.TaxPrice.IntegerValue),
            TradeType.InstantBuy => new TradeProfitValues(
                0d,
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d),
            TradeType.ManualSell => new TradeProfitValues(
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d,
                0d),
            TradeType.ManualBuy => new TradeProfitValues(
                0d,
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d),
            TradeType.Crafting => new TradeProfitValues(
                0d,
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d),
            _ => TradeProfitValues.Empty
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

    private readonly record struct TradeProfitBucketPeriod(DateTime PeriodStart, DateTime PeriodEnd);

    private readonly record struct TradeProfitValues(double Sold, double Bought, double Tax)
    {
        public static TradeProfitValues Empty => new(0d, 0d, 0d);
    }
}