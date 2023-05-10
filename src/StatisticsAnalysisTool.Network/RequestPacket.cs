namespace StatisticsAnalysisTool.Network;

public class RequestPacket
{
    public RequestPacket(short operationCode, Dictionary<byte, object> parameters)
    {
        OperationCode = operationCode;
        Parameters = parameters;
    }

    public short OperationCode { get; }
    public Dictionary<byte, object> Parameters { get; }
}