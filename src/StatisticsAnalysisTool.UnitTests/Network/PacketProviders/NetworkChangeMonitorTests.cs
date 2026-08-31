using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Network.PacketProviders;

namespace StatisticsAnalysisTool.UnitTests.Network.PacketProviders;

[TestFixture]
public class NetworkChangeMonitorTests
{
    private static readonly TimeSpan TestTimerInterval = TimeSpan.FromHours(1);

    [Test]
    public void CheckForChanges_WithChangedSnapshot_ShouldNotifyOncePerState()
    {
        var snapshot = "initial";
        var notificationCount = 0;
        using var monitor = CreateMonitor(() => snapshot, () => notificationCount++);

        monitor.Start();

        monitor.CheckForChanges().Should().BeFalse();
        snapshot = "changed";
        monitor.CheckForChanges().Should().BeTrue();
        monitor.CheckForChanges().Should().BeFalse();
        notificationCount.Should().Be(1);
    }

    [Test]
    public void CheckForChanges_AfterStop_ShouldNotNotify()
    {
        var snapshot = "initial";
        var notificationCount = 0;
        using var monitor = CreateMonitor(() => snapshot, () => notificationCount++);

        monitor.Start();
        monitor.Stop();
        snapshot = "changed";

        monitor.CheckForChanges().Should().BeFalse();
        notificationCount.Should().Be(0);
    }

    private static NetworkChangeMonitor CreateMonitor(Func<string> snapshotFactory, Action networkChanged)
    {
        return new NetworkChangeMonitor(
            networkChanged,
            snapshotFactory,
            TestTimerInterval,
            TestTimerInterval);
    }
}