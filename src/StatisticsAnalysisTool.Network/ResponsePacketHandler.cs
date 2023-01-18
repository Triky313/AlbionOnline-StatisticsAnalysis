namespace StatisticsAnalysisTool.Network;

public abstract class ResponsePacketHandler<TOperation> : PacketHandler<ResponsePacket>
{
    private readonly int _operationCode;

    protected ResponsePacketHandler(int operationCode)
    {
        _operationCode = operationCode;
    }

    protected abstract Task OnActionAsync(TOperation value);

    protected override Task OnHandleAsync(ResponsePacket packet)
    {
        if (_operationCode != packet.OperationCode)
        {
            return NextAsync(packet);
        }

        TOperation instance = (TOperation) Activator.CreateInstance(typeof(TOperation), packet.Parameters);

        return OnActionAsync(instance ?? throw new InvalidOperationException());
    }
}