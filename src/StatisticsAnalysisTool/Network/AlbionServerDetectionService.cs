using Serilog;
using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Network;

public class AlbionServerDetectionService
{
    private readonly object _lock = new();
    private static readonly TimeSpan ServerDetectionStabilityDuration = TimeSpan.FromSeconds(5);
    private AlbionServerInfo _candidateServer = AlbionServerRegistry.Unknown;
    private DateTime _candidateFirstSeenUtc = DateTime.MinValue;

    public event EventHandler<AlbionServerChangedEventArgs> ServerChanged;

    public AlbionServerInfo CurrentServer { get; private set; } = AlbionServerRegistry.Unknown;

    public ServerLocation CurrentServerLocation => CurrentServer.ServerLocation;

    public DateTime LastServerPacketReceivedUtc { get; private set; } = DateTime.MinValue;

    public void DetectFromSourceIp(string sourceIp)
    {
        var detectedServer = AlbionServerRegistry.GetBySourceIp(sourceIp);

        if (detectedServer.ServerLocation == ServerLocation.Unknown)
        {
            return;
        }

        LastServerPacketReceivedUtc = DateTime.UtcNow;
        SetCurrentServer(detectedServer);
    }

    private void SetCurrentServer(AlbionServerInfo detectedServer)
    {
        AlbionServerChangedEventArgs eventArgs = null;
        var resetToUnknown = false;
        var detectedServerName = detectedServer.Name;

        lock (_lock)
        {
            if (CurrentServer.ServerLocation == detectedServer.ServerLocation)
            {
                _candidateServer = AlbionServerRegistry.Unknown;
                _candidateFirstSeenUtc = DateTime.MinValue;
                return;
            }

            if (CurrentServer.ServerLocation != ServerLocation.Unknown)
            {
                var previousServer = CurrentServer;
                CurrentServer = AlbionServerRegistry.Unknown;
                _candidateServer = detectedServer;
                _candidateFirstSeenUtc = DateTime.UtcNow;
                eventArgs = new AlbionServerChangedEventArgs(previousServer, CurrentServer);
                resetToUnknown = true;
            }
            else
            {
                eventArgs = TryPromoteStableCandidate(detectedServer);
            }
        }

        if (eventArgs is null)
        {
            return;
        }

        if (resetToUnknown)
        {
            Log.Information("Albion server detection reset to unknown after receiving {Server} packets", detectedServerName);
            ServerChanged?.Invoke(this, eventArgs);
            return;
        }

        Log.Information("Albion server detected: {Server}", detectedServer.Name);
        ServerChanged?.Invoke(this, eventArgs);
    }

    private AlbionServerChangedEventArgs TryPromoteStableCandidate(AlbionServerInfo detectedServer)
    {
        var now = DateTime.UtcNow;

        if (_candidateServer.ServerLocation != detectedServer.ServerLocation)
        {
            _candidateServer = detectedServer;
            _candidateFirstSeenUtc = now;
            return null;
        }

        if (now - _candidateFirstSeenUtc < ServerDetectionStabilityDuration)
        {
            return null;
        }

        var previousServer = CurrentServer;
        CurrentServer = detectedServer;
        _candidateServer = AlbionServerRegistry.Unknown;
        _candidateFirstSeenUtc = DateTime.MinValue;
        return new AlbionServerChangedEventArgs(previousServer, CurrentServer);
    }
}
