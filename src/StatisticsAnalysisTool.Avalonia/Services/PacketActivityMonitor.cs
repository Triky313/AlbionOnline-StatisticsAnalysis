using Serilog;
using StatisticsAnalysisTool.Avalonia.Models;
using System;

namespace StatisticsAnalysisTool.Avalonia.Services;

public sealed class PacketActivityMonitor
{
    private readonly object _syncLock = new();

    private long _totalPackets;
    private long _eventPackets;
    private long _requestPackets;
    private long _responsePackets;
    private string _lastPacket = PacketActivitySnapshot.Empty.LastPacket;

    public event EventHandler<PacketActivitySnapshot>? SnapshotChanged;

    public PacketActivitySnapshot Snapshot
    {
        get
        {
            lock (_syncLock)
            {
                return CreateSnapshot();
            }
        }
    }

    public void RecordEvent(short code)
    {
        RecordPacket("Event", code, ref _eventPackets);
    }

    public void RecordRequest(short code)
    {
        RecordPacket("Request", code, ref _requestPackets);
    }

    public void RecordResponse(short code)
    {
        RecordPacket("Response", code, ref _responsePackets);
    }

    private void RecordPacket(string packetType, int code, ref long packetCounter)
    {
        PacketActivitySnapshot? snapshotToPublish = null;
        bool shouldLogSinglePacket = false;
        bool shouldLogSummary = false;

        lock (_syncLock)
        {
            _totalPackets++;
            packetCounter++;
            _lastPacket = $"{packetType} {code}";

            if (_totalPackets <= 10 || _totalPackets % 25 == 0)
            {
                snapshotToPublish = CreateSnapshot();
            }

            shouldLogSinglePacket = _totalPackets <= 5;
            shouldLogSummary = _totalPackets > 0 && _totalPackets % 100 == 0;
        }

        if (shouldLogSinglePacket)
        {
            Log.Information("Packet received | Type={PacketType} Code={Code} Total={TotalPackets}", packetType, code, snapshotToPublish?.TotalPackets ?? _totalPackets);
        }
        else if (shouldLogSummary && snapshotToPublish is not null)
        {
            Log.Information("Packet summary | Total={TotalPackets} Events={EventPackets} Requests={RequestPackets} Responses={ResponsePackets} Last={LastPacket}",
                snapshotToPublish.TotalPackets,
                snapshotToPublish.EventPackets,
                snapshotToPublish.RequestPackets,
                snapshotToPublish.ResponsePackets,
                snapshotToPublish.LastPacket);
        }

        if (snapshotToPublish is not null)
        {
            SnapshotChanged?.Invoke(this, snapshotToPublish);
        }
    }

    private PacketActivitySnapshot CreateSnapshot()
    {
        return new PacketActivitySnapshot
        {
            TotalPackets = _totalPackets,
            EventPackets = _eventPackets,
            RequestPackets = _requestPackets,
            ResponsePackets = _responsePackets,
            LastPacket = _lastPacket
        };
    }
}
