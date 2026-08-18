using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
public class ServerUserDataCoordinatorTests
{
    [TestCase(ServerLocation.America)]
    [TestCase(ServerLocation.Asia)]
    [TestCase(ServerLocation.Europe)]
    public void IsServerSwitch_WithSameConfirmedServer_ReturnsFalse(ServerLocation serverLocation)
    {
        ServerUserDataCoordinator.IsServerSwitch(serverLocation, serverLocation).Should().BeFalse();
    }

    [Test]
    public void IsServerSwitch_WithInitialServerDetection_ReturnsFalse()
    {
        ServerUserDataCoordinator.IsServerSwitch(ServerLocation.Unknown, ServerLocation.Europe).Should().BeFalse();
    }

    [Test]
    public void IsServerSwitch_WithDifferentConfirmedServers_ReturnsTrue()
    {
        ServerUserDataCoordinator.IsServerSwitch(ServerLocation.America, ServerLocation.Europe).Should().BeTrue();
    }
}
