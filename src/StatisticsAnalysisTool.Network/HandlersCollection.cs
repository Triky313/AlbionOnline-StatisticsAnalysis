namespace StatisticsAnalysisTool.Network;

internal class HandlersCollection
{
    private readonly List<IPacketHandler> _handlers;

    public HandlersCollection()
    {
        _handlers = new List<IPacketHandler>();
    }

    private IPacketHandler Last
    {
        get
        {
            return _handlers.LastOrDefault();
        }
    }

    public void Add<TPacket>(PacketHandler<TPacket> handler)
    {
        handler.SetNext(Last);
        _handlers.Add(handler);
    }

    public async Task HandleAsync(object request)
    {
        if (Last != null)
        {
            await Last.HandleAsync(request);
        }
    }
}