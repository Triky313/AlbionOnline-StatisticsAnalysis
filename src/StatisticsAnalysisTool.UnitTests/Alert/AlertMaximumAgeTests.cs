using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Alert;
using StatisticsAnalysisTool.Models;
using System.Collections;
using System.Windows.Data;
using AlertMonitor = StatisticsAnalysisTool.Alert.Alert;

namespace StatisticsAnalysisTool.UnitTests.Alert;

[TestFixture]
public class AlertMaximumAgeTests
{
    [Test]
    public void AlertRules_UseIndependentMaximumPriceAges()
    {
        var itemsView = new ListCollectionView(new ArrayList());
        var controller = new AlertController(itemsView);
        var alert = new AlertMonitor(controller, new Item());

        alert.SetPriceAlert(true, 10_000, 5);
        alert.SetAvailabilityAlert(true, 30);
        alert.SetBlackMarketBuyOrderAlert(true, 40_000, 120);

        alert.PriceMaximumAgeMinutes.Should().Be(5);
        alert.AvailabilityMaximumAgeMinutes.Should().Be(30);
        alert.BlackMarketMaximumAgeMinutes.Should().Be(120);
    }
}
