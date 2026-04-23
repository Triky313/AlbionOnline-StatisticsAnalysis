using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common.UserSettings;
using System.Text.Json;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
public class SettingsObjectTests
{
    [Test]
    public void SettingsObject_WithNetworkDevices_ShouldSerializeAndDeserialize()
    {
        var settings = new SettingsObject
        {
            NetworkDevices =
            [
                new NetworkDeviceSettingsObject
                {
                    Identifier = "\\Device\\NPF_Test",
                    Name = "Test network adapter",
                    IsSelected = true
                }
            ]
        };

        var json = JsonSerializer.Serialize(settings);
        var result = JsonSerializer.Deserialize<SettingsObject>(json);

        result.Should().NotBeNull();
        result!.NetworkDevices.Should().ContainSingle();
        result.NetworkDevices[0].Identifier.Should().Be("\\Device\\NPF_Test");
        result.NetworkDevices[0].Name.Should().Be("Test network adapter");
        result.NetworkDevices[0].IsSelected.Should().BeTrue();
    }
}