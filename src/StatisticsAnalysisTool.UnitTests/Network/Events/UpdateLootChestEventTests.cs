using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.UnitTests.Network.Events;

[TestFixture]
public class UpdateLootChestEventTests
{
    [Test]
    public void Constructor_WithCurrentParameters_ParsesByteArrayGuidLists()
    {
        var firstGuidBytes = new byte[]
        {
            138, 61, 186, 72, 17, 149, 121, 72, 154, 110, 20, 231, 64, 20, 106, 2
        };

        var secondGuidBytes = new byte[]
        {
            211, 221, 124, 14, 233, 103, 215, 74, 185, 66, 67, 20, 244, 60, 44, 155
        };

        var parameters = new Dictionary<byte, object>
        {
            { 0, 145 },
            { 1, 7 },
            {
                3,
                new byte[]
                {
                    138, 61, 186, 72, 17, 149, 121, 72, 154, 110, 20, 231, 64, 20, 106, 2
                }
            },
            {
                5,
                new byte[]
                {
                    211, 221, 124, 14, 233, 103, 215, 74, 185, 66, 67, 20, 244, 60, 44, 155
                }
            },
            { 10, 3.70968 },
            { 13, 2 }
        };

        var updateLootChestEvent = new UpdateLootChestEvent(parameters);

        updateLootChestEvent.ObjectId.Should().Be(145);
        updateLootChestEvent.LootFactor.Should().Be(3.70968);
        updateLootChestEvent.IsOpened.Should().BeTrue();
        updateLootChestEvent.Rarity.Should().Be(TreasureRarity.Rare);
        updateLootChestEvent.PlayerGuid.Should().ContainSingle().Which.Should().Be(new Guid(firstGuidBytes));
        updateLootChestEvent.PlayerGuid2.Should().ContainSingle().Which.Should().Be(new Guid(secondGuidBytes));
    }

    [Test]
    public void Constructor_WithLegacyParameters_ParsesObjectArrayGuidLists()
    {
        var firstGuidBytes = new byte[]
        {
            138, 61, 186, 72, 17, 149, 121, 72, 154, 110, 20, 231, 64, 20, 106, 2
        };

        var secondGuidBytes = new byte[]
        {
            211, 221, 124, 14, 233, 103, 215, 74, 185, 66, 67, 20, 244, 60, 44, 155
        };

        var parameters = new Dictionary<byte, object>
        {
            { 0, 145 },
            { 3, new object[] { firstGuidBytes } },
            { 4, new object[] { secondGuidBytes } }
        };

        var updateLootChestEvent = new UpdateLootChestEvent(parameters);

        updateLootChestEvent.IsOpened.Should().BeTrue();
        updateLootChestEvent.PlayerGuid.Should().ContainSingle().Which.Should().Be(new Guid(firstGuidBytes));
        updateLootChestEvent.PlayerGuid2.Should().ContainSingle().Which.Should().Be(new Guid(secondGuidBytes));
    }
}
