using System.Collections.Generic;

namespace Protocol16.Photon
{
    public class OperationRequest
    {
        public byte OperationCode { get; }
        public Dictionary<byte, object> Parameters { get; }

        public OperationRequest(byte operationCode, Dictionary<byte, object> parameters)
        {
            OperationCode = operationCode;
            Parameters = parameters;
        }
    }
}
