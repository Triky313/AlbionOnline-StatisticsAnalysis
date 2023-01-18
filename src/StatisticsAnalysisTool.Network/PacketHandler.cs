namespace StatisticsAnalysisTool.Network;

public abstract class PacketHandler<TPacket> : IPacketHandler
{
    private IPacketHandler _nextHandler;

    public IPacketHandler SetNext(IPacketHandler handler)
    {
        _nextHandler = handler;

        return handler;
    }

    public Task HandleAsync(object request)
    {
        if (request is TPacket packet)
        {
            return OnHandleAsync(packet);
        }

        return _nextHandler != null ? NextAsync(request) : Task.CompletedTask;
    }

    protected abstract Task OnHandleAsync(TPacket packet);

    protected Task NextAsync(object request)
    {
        return _nextHandler?.HandleAsync(request) ?? Task.CompletedTask;
    }
}