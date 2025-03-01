namespace StatisticsAnalysisTool.Protocol16.Photon;

public class EventData
{
    public byte Code { get; }
    public Dictionary<byte, object> Parameters { get; }

    public EventData(byte code, Dictionary<byte, object> parameters)
    {
        Code = code;
        Parameters = parameters;
    }
}