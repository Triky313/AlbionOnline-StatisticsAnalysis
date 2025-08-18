using StatisticsAnalysisTool.PhotonPackageParser;

namespace StatisticsAnalysisTool.Network;

internal sealed class AlbionParser : PhotonParser
{
    private readonly HandlersCollection _handlers = new();

    public void AddHandler<TPacket>(PacketHandler<TPacket> handler)
    {
        _handlers.Add(handler);
    }

    protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
    {
        short eventCode = ParseEventCode(parameters);

        if (eventCode <= -1)
        {
            return;
        }

        var eventPacket = new EventPacket(eventCode, parameters);

        _ = _handlers.HandleAsync(eventPacket);
    }

    protected override void OnRequest(byte operationCodeByte, Dictionary<byte, object> parameters)
    {
        short operationCode = ParseOperationCode(parameters);

        if (operationCode <= -1)
        {
            return;
        }

        var requestPacket = new RequestPacket(operationCode, parameters);

        _ = _handlers.HandleAsync(requestPacket);
    }

    protected override void OnResponse(byte operationCodeByte, short returnCode, string debugMessage, Dictionary<byte, object> parameters)
    {
        short operationCode = ParseOperationCode(parameters);

        if (operationCode <= -1)
        {
            return;
        }

        var responsePacket = new ResponsePacket(operationCode, parameters);

        _ = _handlers.HandleAsync(responsePacket);
    }

    private static short ParseOperationCode(Dictionary<byte, object> parameters)
    {
        if (!parameters.TryGetValue(253, out object value))
        {
            // Other values are returned as -1 code.
            //throw new InvalidOperationException();
            return -1;
        }

        return (short) value;
    }

    private static short ParseEventCode(Dictionary<byte, object> parameters)
    {
        if (!parameters.TryGetValue(252, out object value))
        {
            // Other values are returned as -1 code.
            //throw new InvalidOperationException();
            return -1;
        }

        return (short) value;
    }
}