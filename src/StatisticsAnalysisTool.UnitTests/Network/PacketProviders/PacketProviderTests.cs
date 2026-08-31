using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Network.PacketProviders;

namespace StatisticsAnalysisTool.UnitTests.Network.PacketProviders;

[TestFixture]
public class PacketProviderTests
{
    [Test]
    public void StartResult_WithActiveCaptureSources_ShouldBeSuccessful()
    {
        var result = PacketProviderStartResult.Success(2);

        result.IsSuccessful.Should().BeTrue();
        result.ActiveCaptureSourceCount.Should().Be(2);
    }

    [Test]
    public void StartResult_WithoutActiveCaptureSources_ShouldFail()
    {
        var result = PacketProviderStartResult.Failed;

        result.IsSuccessful.Should().BeFalse();
        result.ActiveCaptureSourceCount.Should().Be(0);
    }

    [Test]
    public void GameDataDetected_ShouldBeReportedOncePerStart()
    {
        var provider = new TestPacketProvider();
        var notificationCount = 0;
        provider.GameDataDetected += (_, _) => notificationCount++;

        provider.Start();
        provider.ReportGameData();
        provider.ReportGameData();

        notificationCount.Should().Be(1);

        provider.Stop();
        provider.Start();
        provider.ReportGameData();

        notificationCount.Should().Be(2);
    }

    private sealed class TestPacketProvider : PacketProvider
    {
        private bool _isRunning;

        public override bool IsRunning => _isRunning;

        public override PacketProviderStartResult Start()
        {
            ResetGameDataDetectedState();
            _isRunning = true;
            return PacketProviderStartResult.Success(1);
        }

        public override void Stop()
        {
            _isRunning = false;
        }

        public void ReportGameData()
        {
            ReportGameDataDetected();
        }
    }
}