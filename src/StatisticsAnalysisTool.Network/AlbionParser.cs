using PhotonPackageParser;

namespace StatisticsAnalysisTool.Network;

internal sealed class AlbionParser : PhotonParser, IPhotonReceiver
{
    private readonly HandlersCollection _handlers;

    public AlbionParser()
    {
        _handlers = new HandlersCollection();
    }

    public void AddHandler<TPacket>(PacketHandler<TPacket> handler)
    {
        _handlers.Add(handler);
    }

    protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
    {
        if (code == 3)
        {
            parameters.Add(252, EventCodes.Move);
        }

        short eventCode = ParseEventCode(parameters);
        var eventPacket = new EventPacket(eventCode, parameters);

        _ = _handlers.HandleAsync(eventPacket);
    }

    protected override void OnRequest(byte operationCodeByte, Dictionary<byte, object> parameters)
    {
        short operationCode = ParseOperationCode(parameters);
        var requestPacket = new RequestPacket(operationCode, parameters);

        _ = _handlers.HandleAsync(requestPacket);
    }

    protected override void OnResponse(byte operationCodeByte, short returnCode, string debugMessage, Dictionary<byte, object> parameters)
    {
        short operationCode = ParseOperationCode(parameters);
        var responsePacket = new ResponsePacket(operationCode, parameters);

        _ = _handlers.HandleAsync(responsePacket);
    }

    private short ParseOperationCode(Dictionary<byte, object> parameters)
    {
        if (!parameters.TryGetValue(253, out object value))
        {
            throw new InvalidOperationException();
        }

        return (short) value;
    }

    private short ParseEventCode(Dictionary<byte, object> parameters)
    {
        if (!parameters.TryGetValue(252, out object value))
        {
            throw new InvalidOperationException();
        }

        return (short) value;
    }
}