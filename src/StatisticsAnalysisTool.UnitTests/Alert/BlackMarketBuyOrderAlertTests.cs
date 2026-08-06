using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System;
using AlertMonitor = StatisticsAnalysisTool.Alert.Alert;

namespace StatisticsAnalysisTool.UnitTests.Alert;

[TestFixture]
public class BlackMarketBuyOrderAlertTests
{
    private static readonly DateTime ReferenceTime = new(2026, 8, 7, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void FindBlackMarketBuyOrderResponse_AtMinimumPrice_ReturnsResponse()
    {
        var response = CreateMarketResponse("Black Market", 40_000, ReferenceTime);

        var result = AlertMonitor.FindBlackMarketBuyOrderResponse(
            [response],
            40_000,
            5,
            ReferenceTime);

        result.Should().BeSameAs(response);
    }

    [Test]
    public void FindBlackMarketBuyOrderResponse_WithMultipleResponses_ReturnsHighestValidBlackMarketBuyOrder()
    {
        var matchingResponse = CreateMarketResponse("Black Market", 50_000, ReferenceTime.AddMinutes(-1));
        var lowerResponse = CreateMarketResponse("Black Market", 40_000, ReferenceTime);
        var expiredResponse = CreateMarketResponse("Black Market", 80_000, ReferenceTime.AddMinutes(-6));
        var otherCityResponse = CreateMarketResponse("Bridgewatch", 100_000, ReferenceTime);

        var result = AlertMonitor.FindBlackMarketBuyOrderResponse(
            [lowerResponse, expiredResponse, otherCityResponse, matchingResponse],
            40_000,
            5,
            ReferenceTime);

        result.Should().BeSameAs(matchingResponse);
    }

    [Test]
    public void FindBlackMarketBuyOrderResponse_BelowMinimumPrice_ReturnsNoMatch()
    {
        var response = CreateMarketResponse("Black Market", 39_999, ReferenceTime);

        var result = AlertMonitor.FindBlackMarketBuyOrderResponse(
            [response],
            40_000,
            5,
            ReferenceTime);

        result.Should().BeNull();
    }

    [TestCase("Black Market")]
    [TestCase("BlackMarket")]
    [TestCase("@BLACK_MARKET")]
    public void GetMarketLocationByLocationNameOrId_WithBlackMarketName_ReturnsBlackMarket(string locationName)
    {
        locationName.GetMarketLocationByLocationNameOrId()
            .Should().Be(MarketLocation.BlackMarket);
    }

    private static MarketResponse CreateMarketResponse(string city, ulong buyPrice, DateTime reportedAt)
    {
        return new MarketResponse
        {
            City = city,
            BuyPriceMax = buyPrice,
            BuyPriceMaxDate = reportedAt
        };
    }
}
