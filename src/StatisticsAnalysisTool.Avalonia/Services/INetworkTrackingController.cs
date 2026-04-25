using StatisticsAnalysisTool.Avalonia.Models;
using StatisticsAnalysisTool.Network.PacketProviders;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Avalonia.Services;

public interface INetworkTrackingController : IDisposable
{
    event EventHandler<TrackingStateChangedEventArgs>? StateChanged;

    Task StartAsync(NetworkCaptureOptions options);

    Task StopAsync();

    Task RestartAsync(NetworkCaptureOptions options);
}
