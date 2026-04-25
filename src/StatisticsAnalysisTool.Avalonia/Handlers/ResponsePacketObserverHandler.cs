using StatisticsAnalysisTool.Avalonia.Services;
using StatisticsAnalysisTool.Network;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Avalonia.Handlers;

public sealed class ResponsePacketObserverHandler(PacketActivityMonitor packetActivityMonitor) : PacketHandler<ResponsePacket>
{
    private readonly PacketActivityMonitor _packetActivityMonitor = packetActivityMonitor;

    protected override Task OnHandleAsync(ResponsePacket packet)
    {
        _packetActivityMonitor.RecordResponse(packet.OperationCode);
        return NextAsync(packet);
    }
}
