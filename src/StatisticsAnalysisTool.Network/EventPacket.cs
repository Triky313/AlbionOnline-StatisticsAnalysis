namespace StatisticsAnalysisTool.Network;

public class EventPacket
{
    public EventPacket(short eventCode, Dictionary<byte, object> parameters)
    {
        EventCode = eventCode;
        Parameters = parameters;
    }

    public short EventCode { get; }
    public Dictionary<byte, object> Parameters { get; }
}