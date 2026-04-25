using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;

namespace StatisticsAnalysisTool.UnitTests.Trade;

[TestFixture]
public class TradeProfitTimeSeriesServiceTests
{
    private readonly TradeProfitTimeSeriesService _service = new();

    [TestCase(TradeProfitTimeAggregation.Hour, 60)]
    [TestCase(TradeProfitTimeAggregation.Day, 24)]
    [TestCase(TradeProfitTimeAggregation.Week, 7)]
    [TestCase(TradeProfitTimeAggregation.Month, 31)]
    [TestCase(TradeProfitTimeAggregation.Year, 12)]
    public void BuildTimeSeries_WithManualAggregation_ReturnsExpectedFixedBucketCount(TradeProfitTimeAggregation aggregation, int expectedBucketCount)
    {
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 3);

        var result = _service.BuildTimeSeries([], from, to, aggregation);

        result.EffectiveAggregation.Should().Be(aggregation);
        result.Points.Should().HaveCount(expectedBucketCount);
        result.Points.Should().OnlyContain(point => point.NetProfit == 0d && point.TradeCount == 0);
    }

    [Test]
    public void BuildTimeSeries_WithDayAggregation_AnchorsBucketsAtRangeEndDay()
    {
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 3);

        var result = _service.BuildTimeSeries([], from, to, TradeProfitTimeAggregation.Day);

        result.Points[0].PeriodStart.Should().Be(new DateTime(2026, 4, 3, 0, 0, 0));
        result.Points[^1].PeriodEnd.Should().Be(new DateTime(2026, 4, 3, 23, 59, 59, 999).AddTicks(9999));
    }

    [Test]
    public void BuildTimeSeries_WithYearAggregation_AnchorsBucketsAtRangeEndMonth()
    {
        var from = new DateTime(2024, 1, 1);
        var to = new DateTime(2026, 4, 25);

        var result = _service.BuildTimeSeries([], from, to, TradeProfitTimeAggregation.Year);

        result.EffectiveAggregation.Should().Be(TradeProfitTimeAggregation.Year);
        result.Points.Should().HaveCount(12);
        result.Points[0].PeriodStart.Should().Be(new DateTime(2025, 5, 1));
        result.Points[^1].PeriodEnd.Should().Be(new DateTime(2026, 4, 30, 23, 59, 59, 999).AddTicks(9999));
    }

    [Test]
    public void BuildTimeSeries_WithSevenDayRangeAndAutoAggregation_UsesHourBuckets()
    {
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 7);

        var result = _service.BuildTimeSeries([], from, to, TradeProfitTimeAggregation.Auto);

        result.EffectiveAggregation.Should().Be(TradeProfitTimeAggregation.Hour);
        result.Points.Should().HaveCount(168);
    }

    [Test]
    public void BuildTimeSeries_WithMixedTradesAndWeekAggregation_GroupsSoldBoughtTaxAndNetProfitCorrectly()
    {
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 2);
        var trades = new[]
        {
            CreateSellMail(new DateTime(2026, 4, 1, 9, 0, 0), 1_000, 50, 10),
            CreateBuyMail(new DateTime(2026, 4, 1, 11, 0, 0), 400, 20, 0),
            CreateInstantSell(new DateTime(2026, 4, 1, 14, 0, 0), 500, 25),
            CreateManualBuy(new DateTime(2026, 4, 2, 10, 0, 0), 300),
            CreateCrafting(new DateTime(2026, 4, 2, 18, 0, 0), 150)
        };

        var result = _service.BuildTimeSeries(trades, from, to, TradeProfitTimeAggregation.Week);
        var firstOfAprilBucket = result.Points.Single(point => point.PeriodStart == new DateTime(2026, 4, 1));
        var secondOfAprilBucket = result.Points.Single(point => point.PeriodStart == new DateTime(2026, 4, 2));

        result.Points.Should().HaveCount(7);
        result.Points.Count(point => point.TradeCount > 0).Should().Be(2);

        firstOfAprilBucket.Sold.Should().Be(1_500);
        firstOfAprilBucket.Bought.Should().Be(400);
        firstOfAprilBucket.Tax.Should().Be(805);
        firstOfAprilBucket.NetProfit.Should().Be(295);
        firstOfAprilBucket.TradeCount.Should().Be(3);
        firstOfAprilBucket.AverageProfitPerTrade.Should().BeApproximately(98.3333333, 0.0001);

        secondOfAprilBucket.Sold.Should().Be(0);
        secondOfAprilBucket.Bought.Should().Be(450);
        secondOfAprilBucket.Tax.Should().Be(0);
        secondOfAprilBucket.NetProfit.Should().Be(-450);
        secondOfAprilBucket.TradeCount.Should().Be(2);
        secondOfAprilBucket.CumulativeNetProfit.Should().Be(-155);
    }

    [Test]
    public void BuildTimeSeries_WithLargeRange_NeverReturnsMoreThanMaxVisiblePoints()
    {
        var from = new DateTime(2010, 1, 1);
        var to = new DateTime(2026, 4, 24);

        var result = _service.BuildTimeSeries([], from, to, TradeProfitTimeAggregation.Auto);

        result.Points.Count.Should().BeLessThanOrEqualTo(TradeProfitTimeSeriesService.MaxVisiblePoints);
    }

    private static global::StatisticsAnalysisTool.Trade.Trade CreateSellMail(DateTime timestamp, long totalPrice, double taxRate, double setupTaxRate)
    {
        return new global::StatisticsAnalysisTool.Trade.Trade
        {
            Id = timestamp.Ticks,
            Ticks = timestamp.Ticks,
            Type = TradeType.Mail,
            MailTypeText = "MARKETPLACE_SELLORDER_FINISHED_SUMMARY",
            MailContent = new MailContent
            {
                InternalTotalPriceWithoutTax = totalPrice * 10_000,
                TaxRate = taxRate,
                TaxSetupRate = setupTaxRate,
                UsedQuantity = 1
            }
        };
    }

    private static global::StatisticsAnalysisTool.Trade.Trade CreateBuyMail(DateTime timestamp, long totalPrice, double taxRate, double setupTaxRate)
    {
        return new global::StatisticsAnalysisTool.Trade.Trade
        {
            Id = timestamp.Ticks,
            Ticks = timestamp.Ticks,
            Type = TradeType.Mail,
            MailTypeText = "MARKETPLACE_BUYORDER_FINISHED_SUMMARY",
            MailContent = new MailContent
            {
                InternalTotalPriceWithoutTax = totalPrice * 10_000,
                TaxRate = taxRate,
                TaxSetupRate = setupTaxRate,
                UsedQuantity = 1
            }
        };
    }

    private static global::StatisticsAnalysisTool.Trade.Trade CreateInstantSell(DateTime timestamp, long totalPrice, double taxRate)
    {
        return new global::StatisticsAnalysisTool.Trade.Trade
        {
            Id = timestamp.Ticks,
            Ticks = timestamp.Ticks,
            Type = TradeType.InstantSell,
            InstantBuySellContent = new InstantBuySellContent
            {
                InternalUnitPrice = totalPrice * 10_000,
                Quantity = 1,
                TaxRate = taxRate
            }
        };
    }

    private static global::StatisticsAnalysisTool.Trade.Trade CreateManualBuy(DateTime timestamp, long totalPrice)
    {
        return new global::StatisticsAnalysisTool.Trade.Trade
        {
            Id = timestamp.Ticks,
            Ticks = timestamp.Ticks,
            Type = TradeType.ManualBuy,
            InstantBuySellContent = new InstantBuySellContent
            {
                InternalUnitPrice = totalPrice * 10_000,
                Quantity = 1
            }
        };
    }

    private static global::StatisticsAnalysisTool.Trade.Trade CreateCrafting(DateTime timestamp, long totalPrice)
    {
        return new global::StatisticsAnalysisTool.Trade.Trade
        {
            Id = timestamp.Ticks,
            Ticks = timestamp.Ticks,
            Type = TradeType.Crafting,
            InstantBuySellContent = new InstantBuySellContent
            {
                InternalUnitPrice = totalPrice * 10_000,
                Quantity = 1
            }
        };
    }
}