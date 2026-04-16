using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.UnitTests.Network.Events;

[TestFixture]
public class UpdateCurrencyEventTests
{
    [Test]
    public void Constructor_WithIntCityFactionParameter_SetsFortSterlingFaction()
    {
        var parameters = new Dictionary<byte, object>
        {
            { 2, 3 }
        };

        var result = new UpdateCurrencyEvent(parameters);

        result.CityFaction.Should().Be(CityFaction.FortSterling);
    }
}