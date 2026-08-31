namespace StatisticsAnalysisTool.Network;

public abstract class ResponsePacketHandler<TOperation> : PacketHandler<ResponsePacket>
{
    private static readonly Func<Dictionary<byte, object>, TOperation> Factory = PacketModelFactory<TOperation>.Factory;

    protected ResponsePacketHandler(int operationCode) : base(operationCode)
    {
    }

    protected abstract Task OnActionAsync(TOperation value);

    protected override Task OnHandleAsync(ResponsePacket packet)
    {
        return OnActionAsync(Factory(packet.Parameters));
    }
}