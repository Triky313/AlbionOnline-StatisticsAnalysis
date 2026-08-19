namespace StatisticsAnalysisTool.Network;

public abstract class PacketHandler<TPacket> : IPacketHandler<TPacket>
{
    protected PacketHandler(int code)
    {
        Code = code;
    }

    public int Code { get; }

    public Task HandleAsync(TPacket packet)
    {
        return OnHandleAsync(packet);
    }

    protected abstract Task OnHandleAsync(TPacket packet);
}