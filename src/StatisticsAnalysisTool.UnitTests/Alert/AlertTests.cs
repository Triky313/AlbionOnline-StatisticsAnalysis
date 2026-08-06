using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using AlertMonitor = StatisticsAnalysisTool.Alert.Alert;

namespace StatisticsAnalysisTool.UnitTests.Alert;

[TestFixture]
public class AlertTests
{
    private static readonly DateTime ReferenceTime = new(2026, 8, 7, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task WaitForNextPollAsync_WhenCanceled_CompletesWithoutException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var waitTask = AlertMonitor.WaitForNextPollAsync(
            TimeSpan.FromMinutes(1),
            cancellationTokenSource.Token);

        cancellationTokenSource.Cancel();

        var shouldContinue = await waitTask;

        shouldContinue.Should().BeFalse();
    }

    [Test]
    public void FindPriceThresholdResponse_WithinConfiguredAge_ReturnsLowestMatchingPrice()
    {
        var matchingResponse = CreateMarketResponse(
            "Bridgewatch",
            4_999,
            ReferenceTime.AddDays(-14));
        var moreExpensiveResponse = CreateMarketResponse(
            "Lymhurst",
            6_000,
            ReferenceTime);
        var blackMarketResponse = CreateMarketResponse(
            "Black Market",
            1_000,
            ReferenceTime);

        var result = AlertMonitor.FindPriceThresholdResponse(
            [matchingResponse, moreExpensiveResponse, blackMarketResponse],
            5_000,
            21 * 24 * 60,
            ReferenceTime);

        result.Should().BeSameAs(matchingResponse);
    }

    [Test]
    public void FindPriceThresholdResponse_OlderThanConfiguredAge_ReturnsNoMatch()
    {
        var expiredResponse = CreateMarketResponse(
            "Bridgewatch",
            4_999,
            ReferenceTime.AddMinutes(-6));

        var result = AlertMonitor.FindPriceThresholdResponse(
            [expiredResponse],
            5_000,
            5,
            ReferenceTime);

        result.Should().BeNull();
    }

    [Test]
    public void FindAvailabilityResponse_WithValidPrices_ReturnsMostRecentlyReportedPrice()
    {
        var olderResponse = CreateMarketResponse(
            "Bridgewatch",
            5_000,
            ReferenceTime.AddDays(-14));
        var newestResponse = CreateMarketResponse(
            "Lymhurst",
            7_000,
            ReferenceTime.AddDays(-1));
        var missingPriceResponse = CreateMarketResponse(
            "Martlock",
            0,
            ReferenceTime);
        var blackMarketResponse = CreateMarketResponse(
            "Black Market",
            1_000,
            ReferenceTime);

        var result = AlertMonitor.FindAvailabilityResponse(
            [olderResponse, newestResponse, missingPriceResponse, blackMarketResponse],
            21 * 24 * 60,
            ReferenceTime);

        result.Should().BeSameAs(newestResponse);
    }

    private static MarketResponse CreateMarketResponse(string city, ulong sellPrice, DateTime reportedAt)
    {
        return new MarketResponse
        {
            City = city,
            SellPriceMin = sellPrice,
            SellPriceMinDate = reportedAt
        };
    }
}
