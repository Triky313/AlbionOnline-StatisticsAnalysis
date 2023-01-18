namespace StatisticsAnalysisTool.Network;

public abstract class EventPacketHandler<TEvent> : PacketHandler<EventPacket>
{
    private readonly int _eventCode;

    protected EventPacketHandler(int eventCode)
    {
        _eventCode = eventCode;
    }

    protected abstract Task OnActionAsync(TEvent value);

    protected override Task OnHandleAsync(EventPacket packet)
    {
        if (_eventCode != packet.EventCode)
        {
            return NextAsync(packet);
        }

        TEvent instance = (TEvent) Activator.CreateInstance(typeof(TEvent), packet.Parameters);

        return OnActionAsync(instance ?? throw new InvalidOperationException());
    }
}