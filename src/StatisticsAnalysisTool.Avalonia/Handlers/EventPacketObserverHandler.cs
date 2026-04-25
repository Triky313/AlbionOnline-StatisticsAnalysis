using StatisticsAnalysisTool.Avalonia.Services;
using StatisticsAnalysisTool.Network;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Avalonia.Handlers;

public sealed class EventPacketObserverHandler(PacketActivityMonitor packetActivityMonitor) : PacketHandler<EventPacket>
{
    private readonly PacketActivityMonitor _packetActivityMonitor = packetActivityMonitor;

    protected override Task OnHandleAsync(EventPacket packet)
    {
        _packetActivityMonitor.RecordEvent(packet.EventCode);
        return NextAsync(packet);
    }
}
