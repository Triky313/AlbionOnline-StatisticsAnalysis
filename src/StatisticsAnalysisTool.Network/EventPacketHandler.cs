namespace StatisticsAnalysisTool.Network;

public abstract class EventPacketHandler<TEvent> : PacketHandler<EventPacket>
{
    private static readonly Func<Dictionary<byte, object>, TEvent> Factory = PacketModelFactory<TEvent>.Factory;

    protected EventPacketHandler(int eventCode) : base(eventCode)
    {
    }

    protected abstract Task OnActionAsync(TEvent value);

    protected override Task OnHandleAsync(EventPacket packet)
    {
        return OnActionAsync(Factory(packet.Parameters));
    }
}