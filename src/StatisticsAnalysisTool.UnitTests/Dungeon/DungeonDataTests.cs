using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Dungeon;

namespace StatisticsAnalysisTool.UnitTests.Dungeon;

[TestFixture]
public class DungeonDataTests
{
    [TestCase("T5_RANDOM_DUNGEON_SOLO_LEVEL1", DungeonMode.Solo)]
    [TestCase("T6_RANDOM_DUNGEON_VETERAN_LEVEL1", DungeonMode.Standard)]
    [TestCase("T8_AVALON_RANDOM_DUNGEON", DungeonMode.Avalon)]
    public void GetDungeonMode_WithMultipleIdentifiers_ReturnsFirstRecognizedMode(
        string dungeonIdentifier,
        DungeonMode expectedMode)
    {
        var result = DungeonData.GetDungeonMode("UNRECOGNIZED_CLUSTER", dungeonIdentifier);
        result.Should().Be(expectedMode);
    }

    [Test]
    public void GetDungeonMode_WithUnknownIdentifiers_ReturnsUnknown()
    {
        var result = DungeonData.GetDungeonMode("UNRECOGNIZED_CLUSTER", string.Empty);
        result.Should().Be(DungeonMode.Unknown);
    }
}
