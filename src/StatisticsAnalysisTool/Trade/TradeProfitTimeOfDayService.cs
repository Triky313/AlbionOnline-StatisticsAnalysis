using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeProfitTimeOfDayService
{
    private static readonly DayOfWeek[] OrderedDays =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    ];

    private readonly TradeAnalyticsValueService _tradeAnalyticsValueService = new();

    public TradeProfitTimeOfDayResult Build(IEnumerable<Trade> trades)
    {
        var heatmapBuckets = CreateHeatmapBuckets();
        var hourlyBuckets = CreateHourlyBuckets();

        foreach (var trade in trades ?? [])
        {
            if (trade == null)
            {
                continue;
            }

            var breakdown = _tradeAnalyticsValueService.GetBreakdown(trade);
            if (!HasRelevantValues(breakdown))
            {
                continue;
            }

            var tradeTimestamp = trade.Timestamp;
            var hour = tradeTimestamp.Hour;
            var dayOfWeek = NormalizeDayOfWeek(tradeTimestamp.DayOfWeek);

            heatmapBuckets[(dayOfWeek, hour)].Add(breakdown);
            hourlyBuckets[hour].Add(breakdown);
        }

        return new TradeProfitTimeOfDayResult
        {
            HeatmapPoints = BuildHeatmapPoints(heatmapBuckets),
            HourlyPoints = BuildHourlyPoints(hourlyBuckets)
        };
    }

    private static Dictionary<(DayOfWeek DayOfWeek, int Hour), TradeProfitTimeOfDayAccumulator> CreateHeatmapBuckets()
    {
        var buckets = new Dictionary<(DayOfWeek DayOfWeek, int Hour), TradeProfitTimeOfDayAccumulator>(OrderedDays.Length * 24);

        foreach (var dayOfWeek in OrderedDays)
        {
            for (var hour = 0; hour < 24; hour++)
            {
                buckets[(dayOfWeek, hour)] = new TradeProfitTimeOfDayAccumulator(dayOfWeek, hour);
            }
        }

        return buckets;
    }

    private static Dictionary<int, TradeProfitTimeOfDayAccumulator> CreateHourlyBuckets()
    {
        var buckets = new Dictionary<int, TradeProfitTimeOfDayAccumulator>(24);

        for (var hour = 0; hour < 24; hour++)
        {
            buckets[hour] = new TradeProfitTimeOfDayAccumulator(null, hour);
        }

        return buckets;
    }

    private static bool HasRelevantValues(TradeAnalyticsValueBreakdown breakdown)
    {
        return breakdown.Sold > 0d ||
               breakdown.Bought > 0d ||
               breakdown.Tax > 0d ||
               breakdown.SoldQuantity > 0 ||
               breakdown.BoughtQuantity > 0;
    }

    private static DayOfWeek NormalizeDayOfWeek(DayOfWeek dayOfWeek)
    {
        return OrderedDays.Contains(dayOfWeek) ? dayOfWeek : DayOfWeek.Sunday;
    }

    private static IReadOnlyList<TradeProfitTimeOfDayPoint> BuildHeatmapPoints(Dictionary<(DayOfWeek DayOfWeek, int Hour), TradeProfitTimeOfDayAccumulator> buckets)
    {
        var points = new List<TradeProfitTimeOfDayPoint>(OrderedDays.Length * 24);

        foreach (var dayOfWeek in OrderedDays)
        {
            for (var hour = 0; hour < 24; hour++)
            {
                points.Add(buckets[(dayOfWeek, hour)].ToPoint());
            }
        }

        return points;
    }

    private static IReadOnlyList<TradeProfitTimeOfDayPoint> BuildHourlyPoints(Dictionary<int, TradeProfitTimeOfDayAccumulator> buckets)
    {
        var points = new List<TradeProfitTimeOfDayPoint>(24);

        for (var hour = 0; hour < 24; hour++)
        {
            points.Add(buckets[hour].ToPoint());
        }

        return points;
    }

    private sealed class TradeProfitTimeOfDayAccumulator(DayOfWeek? dayOfWeek, int hour)
    {
        public DayOfWeek? DayOfWeek
        {
            get;
        } = dayOfWeek;

        public int Hour
        {
            get;
        } = hour;

        public double Sold
        {
            get;
            private set;
        }

        public double Bought
        {
            get;
            private set;
        }

        public double Tax
        {
            get;
            private set;
        }

        public int TradeCount
        {
            get;
            private set;
        }

        public void Add(TradeAnalyticsValueBreakdown breakdown)
        {
            Sold += breakdown.Sold;
            Bought += breakdown.Bought;
            Tax += breakdown.Tax;
            TradeCount++;
        }

        public TradeProfitTimeOfDayPoint ToPoint()
        {
            var netProfit = Sold - Bought - Tax;

            return new TradeProfitTimeOfDayPoint
            {
                Hour = Hour,
                DayOfWeek = DayOfWeek,
                Sold = Sold,
                Bought = Bought,
                Tax = Tax,
                NetProfit = netProfit,
                AverageNetProfitPerTrade = TradeCount > 0 ? netProfit / TradeCount : 0d,
                TradeCount = TradeCount
            };
        }
    }
}