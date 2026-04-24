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

    [Test]
    public void BuildTimeSeries_WithEmptyTradeList_ReturnsZeroBucketsForSelectedRange()
    {
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 3);

        var result = _service.BuildTimeSeries([], from, to, TradeProfitTimeAggregation.Day);

        result.EffectiveAggregation.Should().Be(TradeProfitTimeAggregation.Day);
        result.Points.Should().HaveCount(3);
        result.Points.Should().OnlyContain(x => x.NetProfit == 0d && x.TradeCount == 0);
        result.Points[0].PeriodStart.Should().Be(from);
        result.Points[^1].PeriodEnd.Should().Be(new DateTime(2026, 4, 3, 23, 59, 59, 999).AddTicks(9999));
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
    public void BuildTimeSeries_WithManualAggregationAbovePointLimit_FallsBackToCoarserAggregation()
    {
        var from = new DateTime(2020, 1, 1);
        var to = new DateTime(2021, 12, 31);

        var result = _service.BuildTimeSeries([], from, to, TradeProfitTimeAggregation.Day);

        result.EffectiveAggregation.Should().Be(TradeProfitTimeAggregation.Week);
        result.Points.Count.Should().BeLessThanOrEqualTo(TradeProfitTimeSeriesService.MaxVisiblePoints);
    }

    [Test]
    public void BuildTimeSeries_WithMixedTrades_GroupsSoldBoughtTaxAndNetProfitCorrectly()
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

        var result = _service.BuildTimeSeries(trades, from, to, TradeProfitTimeAggregation.Day);

        result.Points.Should().HaveCount(2);
        result.Points[0].Sold.Should().Be(1_500);
        result.Points[0].Bought.Should().Be(400);
        result.Points[0].Tax.Should().Be(105);
        result.Points[0].NetProfit.Should().Be(995);
        result.Points[0].TradeCount.Should().Be(3);
        result.Points[0].AverageProfitPerTrade.Should().BeApproximately(331.6666667, 0.0001);

        result.Points[1].Sold.Should().Be(0);
        result.Points[1].Bought.Should().Be(450);
        result.Points[1].Tax.Should().Be(0);
        result.Points[1].NetProfit.Should().Be(-450);
        result.Points[1].TradeCount.Should().Be(2);
        result.Points[1].CumulativeNetProfit.Should().Be(545);
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
            MailTypeText = MailType.MarketplaceSellOrderFinished.ToString(),
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
            MailTypeText = MailType.MarketplaceBuyOrderFinished.ToString(),
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