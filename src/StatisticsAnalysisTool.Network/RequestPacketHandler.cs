namespace StatisticsAnalysisTool.Network;

public abstract class RequestPacketHandler<TOperation> : PacketHandler<RequestPacket>
{
    private static readonly Func<Dictionary<byte, object>, TOperation> Factory = PacketModelFactory<TOperation>.Factory;

    protected RequestPacketHandler(int operationCode) : base(operationCode)
    {
    }

    protected abstract Task OnActionAsync(TOperation value);

    protected override Task OnHandleAsync(RequestPacket packet)
    {
        return OnActionAsync(Factory(packet.Parameters));
    }
}