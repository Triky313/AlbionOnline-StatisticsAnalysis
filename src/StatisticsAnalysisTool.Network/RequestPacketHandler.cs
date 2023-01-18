namespace StatisticsAnalysisTool.Network;

public abstract class RequestPacketHandler<TOperation> : PacketHandler<RequestPacket> where TOperation : BaseOperation
{
    private readonly int _operationCode;

    protected RequestPacketHandler(int operationCode)
    {
        _operationCode = operationCode;
    }

    protected abstract Task OnActionAsync(TOperation value);

    protected override Task OnHandleAsync(RequestPacket packet)
    {
        if (_operationCode != packet.OperationCode)
        {
            return NextAsync(packet);
        }

        TOperation instance = (TOperation)Activator.CreateInstance(typeof(TOperation), packet.Parameters);

        return OnActionAsync(instance ?? throw new InvalidOperationException());
    }
}