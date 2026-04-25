using StatisticsAnalysisTool.Avalonia.Services;
using StatisticsAnalysisTool.Network;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Avalonia.Handlers;

public sealed class RequestPacketObserverHandler(PacketActivityMonitor packetActivityMonitor) : PacketHandler<RequestPacket>
{
    private readonly PacketActivityMonitor _packetActivityMonitor = packetActivityMonitor;

    protected override Task OnHandleAsync(RequestPacket packet)
    {
        _packetActivityMonitor.RecordRequest(packet.OperationCode);
        return NextAsync(packet);
    }
}
