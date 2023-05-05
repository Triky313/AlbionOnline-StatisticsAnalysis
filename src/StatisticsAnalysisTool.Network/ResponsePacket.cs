namespace StatisticsAnalysisTool.Network;

public class ResponsePacket
{
    public ResponsePacket(short operationCode, Dictionary<byte, object> parameters)
    {
        OperationCode = operationCode;
        Parameters = parameters;
    }

    public short OperationCode { get; }
    public Dictionary<byte, object> Parameters { get; }
}