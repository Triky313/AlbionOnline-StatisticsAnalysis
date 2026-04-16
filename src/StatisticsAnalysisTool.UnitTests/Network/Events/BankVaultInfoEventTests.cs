using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.UnitTests.Network.Events;

[TestFixture]
public class BankVaultInfoEventTests
{
    [Test]
    public void Constructor_WithCurrentParameters_ParsesCurrentVaultData()
    {
        var firstGuidBytes = new byte[]
        {
            211, 245, 58, 1, 23, 138, 199, 65, 142, 251, 128, 15, 251, 68, 182, 161
        };

        var secondGuidBytes = new byte[]
        {
            155, 78, 246, 2, 136, 13, 139, 67, 178, 191, 234, 21, 0, 0, 0, 0
        };

        var parameters = new Dictionary<byte, object>
        {
            { 0, 6 },
            { 1, "f56a368d-2f0b-4d01-a1ba-0079cf8b1fa9@4001" },
            {
                2,
                new byte[]
                {
                    211, 245, 58, 1, 23, 138, 199, 65, 142, 251, 128, 15, 251, 68, 182, 161,
                    155, 78, 246, 2, 136, 13, 139, 67, 178, 191, 234, 21, 0, 0, 0, 0
                }
            },
            {
                3,
                new[]
                {
                    "PvP 1 Weapons",
                    "2 Artefakte"
                }
            },
            {
                4,
                new[]
                {
                    "icon_tag_aim",
                    "icon_tag_feather"
                }
            },
            {
                5,
                new[]
                {
                    -1809966081,
                    506219263
                }
            }
        };

        var bankVaultInfoEvent = new BankVaultInfoEvent(parameters);

        bankVaultInfoEvent.ObjectId.Should().Be(6);
        bankVaultInfoEvent.LocationGuidString.Should().Be("f56a368d-2f0b-4d01-a1ba-0079cf8b1fa9@4001");
        bankVaultInfoEvent.VaultGuidList.Should().ContainInOrder(new Guid(firstGuidBytes), new Guid(secondGuidBytes));
        bankVaultInfoEvent.VaultNames.Should().ContainInOrder("PvP 1 Weapons", "2 Artefakte");
        bankVaultInfoEvent.IconTags.Should().ContainInOrder("icon_tag_aim", "icon_tag_feather");
        bankVaultInfoEvent.VaultColors.Should().ContainInOrder(-1809966081, 506219263);
    }
}
