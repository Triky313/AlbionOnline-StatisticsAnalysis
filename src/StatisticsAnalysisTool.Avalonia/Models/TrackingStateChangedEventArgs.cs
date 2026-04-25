using System;

namespace StatisticsAnalysisTool.Avalonia.Models;

public sealed class TrackingStateChangedEventArgs : EventArgs
{
    public TrackingLifecycleStatus Status { get; init; }

    public string ActiveAdapter { get; init; } = string.Empty;

    public string LastError { get; init; } = string.Empty;

    public PacketActivitySnapshot PacketActivity { get; init; } = PacketActivitySnapshot.Empty;
}
