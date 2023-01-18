namespace StatisticsAnalysisTool.Network;

public class ReceiverBuilder
{
    private readonly AlbionParser _parser;

    public ReceiverBuilder()
    {
        _parser = new AlbionParser();
    }

    public static ReceiverBuilder Create()
    {
        return new ReceiverBuilder();
    }

    public void AddHandler<TPacket>(PacketHandler<TPacket> handler)
    {
        _parser.AddHandler(handler);
    }

    public void AddEventHandler<TEvent>(EventPacketHandler<TEvent> handler)
    {
        AddHandler(handler);
    }

    public void AddRequestHandler<TOperation>(RequestPacketHandler<TOperation> handler)
    {
        AddHandler(handler);
    }

    public void AddResponseHandler<TOperation>(ResponsePacketHandler<TOperation> handler)
    {
        AddHandler(handler);
    }

    public IPhotonReceiver Build()
    {
        return _parser;
    }
}