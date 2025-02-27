using System.Collections.Generic;

namespace Protocol16.Photon
{
    public class OperationResponse
    {
        public byte OperationCode { get; }
        public short ReturnCode { get; }
        public string DebugMessage { get; }
        public Dictionary<byte, object> Parameters { get; }

        public OperationResponse(byte operationCode, short returnCode, string debugMessage, Dictionary<byte, object> parameters)
        {
            OperationCode = operationCode;
            ReturnCode = returnCode;
            DebugMessage = debugMessage;
            Parameters = parameters;
        }
    }
}
